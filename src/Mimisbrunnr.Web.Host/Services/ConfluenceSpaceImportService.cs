using System.IO.Compression;
using System.Xml.Linq;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Web.Wiki;
using Mimisbrunnr.Web.Wiki.Import;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;
using ReverseMarkdown;

namespace Mimisbrunnr.Web.Host.Services;

internal class ConfluenceSpaceImportService : ISpaceImportService
{
    private readonly Converter _markdownConverter;
    private readonly IPageManager _pageManager;
    private readonly ISpaceManager _spaceManager;
    private readonly IPermissionService _permissionService;

    public ConfluenceSpaceImportService(
        IPageManager pageManager,
        ISpaceManager spaceManager,
        IPermissionService permissionService
    )
    {
        _pageManager = pageManager;
        _spaceManager = spaceManager;
        _permissionService = permissionService;
        var markdownConfig = new Config
        {
            UnknownTags = Config.UnknownTagsOption.Bypass,
            // generate GitHub flavoured markdown, supported for BR, PRE and table tags
            GithubFlavored = true,
            // will ignore all comments
            RemoveComments = true,
            // remove markdown output for links where appropriate
            SmartHrefHandling = true
        };
        _markdownConverter = new Converter(markdownConfig);
    }

    public async Task Import(SpaceModel spaceModel, Stream importStream, UserInfo createdBy)
    {
        var space = await _spaceManager.GetByKey(spaceModel.Key);
        if (space is null)
            throw new SpaceNotFoundException();

        await _permissionService.EnsureAdminPermission(space.Key, createdBy);

        var entities = await ReadEntitiesFromZip(importStream);
        var entitiesDocument = XDocument.Parse(entities);

        var contents = ParseContents(entitiesDocument);
        var pages = ParsePages(entitiesDocument);
        pages = FilterMostRecent(pages);

        var spaceHomePage = ToPage(pages.FirstOrDefault(x => !x.ContainsKey("parent") && x.ContainsKey("children")), space.Id, contents);

        var flatPagesContracts = pages.Select(x => ToPage(x, space.Id, contents)).ToArray();
        var pageTree = flatPagesContracts.ToModel(spaceHomePage, space);

        await UpdateHomePage(space.HomePageId, spaceHomePage, createdBy);

        foreach (var child in pageTree.Childs)
            await CreatePageFromTree(child, space.Id, space.HomePageId, createdBy);
    }

    private async Task UpdateHomePage(string homePageId, Page newHomePage, UserInfo updatedBy)
    {
        var actualHomePage = await _pageManager.GetById(homePageId);
        actualHomePage.Content = newHomePage.Content;
        actualHomePage.Name = newHomePage.Name;
        actualHomePage.SpaceId = actualHomePage.SpaceId;
        await _pageManager.Update(actualHomePage, updatedBy);
    }

    private async Task CreatePageFromTree(PageTreeModel pageTreeModel, string spaceId, string parentPageId, UserInfo createdBy)
    {
        var page = await _pageManager.Create(
             spaceId,
             pageTreeModel.Page.Name,
             pageTreeModel.Page.Content,
             createdBy,
             parentPageId
        );
        foreach (var child in pageTreeModel.Childs)
            await CreatePageFromTree(child, spaceId, page.Id, createdBy);
    }

    private Page ToPage(IDictionary<string, object> page, string spaceId, IDictionary<string, string> contents)
    {
        var body = "Empty page";
        if (page.ContainsKey("bodyContents"))
        {
            var contentId = ((string[])page["bodyContents"]).First();
            var content = contents[contentId];
            body = _markdownConverter.Convert(ConfluenceContentPreprocessing.Process(content));
        }

        return new Page
        {
            Id = page.ContainsKey("id") ? page["id"]?.ToString() : string.Empty,
            Name = page.ContainsKey("title") ? page["title"]?.ToString() : string.Empty,
            SpaceId = spaceId,
            Content = body,
            ParentId = page.ContainsKey("parent") ? page["parent"]?.ToString() : null
        };
    }

    private static IList<IDictionary<string, object>> ParsePages(XDocument doc)
    {
        var allowedProperty = new string[] { "creatorName", "creationDate", "lastModificationDate", "title", "position", "version", "contentStatus" };
        var allowedCollections = new string[] { "children", "bodyContents" };

        var pages = doc.Descendants().Where(x => x.Name == "object" && x.Attribute("class")?.Value == "Page").ToArray();
        var parsedPages = new List<IDictionary<string, object>>();
        foreach (var page in pages)
        {
            var pageData = new Dictionary<string, object>();
            var pageElements = page.Elements();
            foreach (var element in pageElements)
            {
                var name = element.Attribute("name")?.Value ?? string.Empty;
                if (element.Name.ToString() == "id")
                    pageData.Add("id", element.Value);

                if (element.Name.ToString() == "property")
                    if (allowedProperty.Contains(name))
                        pageData.Add(name, element.Value);
                    else if (name == "parent")
                        pageData.Add(name, element.Elements()?.FirstOrDefault(x => x.Name.ToString() == "id")?.Value);

                if (element.Name.ToString() == "collection")
                    if (allowedCollections.Contains(name))
                    {
                        var ids = element.Elements().Select(x => x.Elements().FirstOrDefault()?.Value)?.ToArray();
                        pageData.Add(name, ids);

                    }

            }
            parsedPages.Add(pageData);
        }
        return parsedPages;
    }

    private static IList<IDictionary<string, object>> FilterMostRecent(IList<IDictionary<string, object>> pages)
    {
        var currentPages = pages.Where(x => x["contentStatus"]?.ToString() == "current").ToArray();
        var latestPages = new Dictionary<string, IDictionary<string, object>>();

        foreach (var page in currentPages)
        {
            var id = $"{page["title"]}#{page["creationDate"]}";
            if (!latestPages.ContainsKey(id))
            {
                latestPages.Add(id, page);
                continue;
            }
            var latestPage = latestPages[id];
            var latestPageLastModified = DateTime.Parse(latestPage["lastModificationDate"].ToString());
            var currentPageLastModified = DateTime.Parse(page["lastModificationDate"].ToString());
            if (latestPageLastModified < currentPageLastModified)
                latestPages[id] = page;
        }

        return latestPages.Values.ToList();
    }

    private static IDictionary<string, string> ParseContents(XDocument doc)
    {
        var bodys = doc.Descendants().Where(x => x.Name == "object" && x.Attribute("class")?.Value == "BodyContent").ToArray();
        var bodyContents = new Dictionary<string, string>();

        foreach (var body in bodys)
        {
            var bodyElements = body.Elements();
            var id = bodyElements.FirstOrDefault(x => x.Name.ToString() == "id")?.Value;
            var content = bodyElements.FirstOrDefault(x => x.Name.ToString() == "property" && x.Attribute("name")?.Value == "body")?.Value;
            if (string.IsNullOrEmpty(id) && string.IsNullOrEmpty(content))
                continue;
            bodyContents.Add(id, content);
        }
        return bodyContents;
    }

    private static Task<string> ReadEntitiesFromZip(Stream zip)
    {
        ZipArchive archive = new ZipArchive(zip);
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            if (entry.FullName.Equals("entities.xml", StringComparison.OrdinalIgnoreCase))
            {
                var reader = new StreamReader(entry.Open());
                return reader.ReadToEndAsync();
            }
        }

        return null;
    }
}
