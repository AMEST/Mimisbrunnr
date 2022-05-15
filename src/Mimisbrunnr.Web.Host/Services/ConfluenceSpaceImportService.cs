using System.IO.Compression;
using System.Xml.Linq;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Web.Wiki;
using Mimisbrunnr.Web.Wiki.Import;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;
using MongoDB.Driver;
using ReverseMarkdown;
using Skidbladnir.Utility.Common;

namespace Mimisbrunnr.Web.Host.Services;

internal class ConfluenceSpaceImportService : ISpaceImportService
{
    private readonly Converter _markdownConverter;
    private readonly IAttachmentManager _attachmentManager;
    private readonly IPageManager _pageManager;
    private readonly ISpaceManager _spaceManager;
    private readonly IPermissionService _permissionService;

    public ConfluenceSpaceImportService(
        IPageManager pageManager,
        ISpaceManager spaceManager,
        IPermissionService permissionService,
        IAttachmentManager attachmentManager
    )
    {
        _attachmentManager = attachmentManager;
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
        using (var archive = new ZipArchive(importStream))
        {
            var entities = await ReadEntitiesFromZip(archive);
            var entitiesDocument = XDocument.Parse(entities);
            entities = null;
            GC.Collect(); // HACK: used manual gc.collect for clean memory (confluence xml haven't structure and hav so trash data )
            var attachments = ParseAttachments(entitiesDocument);
            var contents = ParseContents(entitiesDocument);
            var pages = ParsePages(entitiesDocument);
            entitiesDocument = null;
            GC.Collect();
            
            pages = FilterMostRecent(pages);

            var spaceHomePage = ToPage(pages.FirstOrDefault(x => !x.ContainsKey("parent") && x.ContainsKey("children")), space.Id, contents);
            GC.Collect();

            var flatPagesContracts = pages.Where(x => x.ContainsKey("parent")).Select(x => ToPage(x, space.Id, contents));
            var pageTree = flatPagesContracts.ToModel(spaceHomePage, space);
            flatPagesContracts = null;
            contents.Clear();
            GC.Collect();

            var homePageAttachmentIds = pages.Where(x => x.ContainsKey("parent")).FirstOrDefault(x => x.ContainsKey("id") && ((string)x["id"]) == spaceHomePage.Id && x.ContainsKey("attachments"))
                ?.Where(x => x.Key == "attachments")?.Select(x => x.Value)?.FirstOrDefault() as string[] ?? Array.Empty<string>();
            await UpdateHomePage(space.HomePageId, spaceHomePage, attachments.Where(x => homePageAttachmentIds.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value), archive, createdBy);
            
            foreach (var child in pageTree.Childs)
                await CreatePageFromTree(child, space.Id, space.HomePageId, pages, attachments, archive, createdBy);
            pages.Clear();
            pageTree = null;
            attachments.Clear();
        }
        GC.Collect();
    }

    private async Task UpdateHomePage(string homePageId, Page newHomePage, IDictionary<string, string> attachments, ZipArchive archive, UserInfo updatedBy)
    {
        var actualHomePage = await _pageManager.GetById(homePageId);
        actualHomePage.Content = newHomePage.Content;
        actualHomePage.Name = newHomePage.Name;
        actualHomePage.SpaceId = actualHomePage.SpaceId;
        await _pageManager.Update(actualHomePage, updatedBy);

        if (attachments.Keys.Count == 0) return;
        actualHomePage.Content = ConfluenceContentPreprocessing.PostProcess(actualHomePage.Content, actualHomePage.Id);
        await _pageManager.Update(actualHomePage, updatedBy);

        foreach (var (id, name) in attachments)
            await UploadAttachment(actualHomePage, newHomePage.Id, id, name, archive, updatedBy);
    }

    private async Task CreatePageFromTree(PageTreeModel pageTreeModel, string spaceId, string parentPageId, IList<IDictionary<string, object>> rawPages, IDictionary<string, string> attachments, ZipArchive archive, UserInfo createdBy)
    {

        var page = await _pageManager.Create(
             spaceId,
             pageTreeModel.Page.Name,
             pageTreeModel.Page.Content,
             createdBy,
             parentPageId
        );

        var pageAttachmentsIds = rawPages.Where(x => x.ContainsKey("parent")).FirstOrDefault(x => x.ContainsKey("id") && ((string)x["id"]) == pageTreeModel.Page.Id && x.ContainsKey("attachments"))
            ?.Where(x => x.Key == "attachments")?.Select(x => x.Value)?.FirstOrDefault() as string[] ?? Array.Empty<string>();
        var pageAttachmens = attachments.Where(x => pageAttachmentsIds.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
        if (pageAttachmens.Keys.Count != 0)
        {
            page.Content = ConfluenceContentPreprocessing.PostProcess(page.Content, page.Id);
            await _pageManager.Update(page, createdBy);
            foreach (var (id, name) in pageAttachmens)
                await UploadAttachment(page, pageTreeModel.Page.Id, id, name, archive, createdBy);
        }
        foreach (var child in pageTreeModel.Childs)
            await CreatePageFromTree(child, spaceId, page.Id, rawPages, attachments, archive, createdBy);

    }

    private async Task UploadAttachment(Page actualHomePage, string importPageId, string id, string name, ZipArchive archive, UserInfo updatedBy)
    {
        var attachment = archive.Entries.Where(x => x.FullName.StartsWith($"attachments/{importPageId}/{id}"))?.OrderByDescending(x => x.FullName)?.FirstOrDefault();
        if (attachment == null) return;

        using var ms = new MemoryStream();
        using (var attachmentStream = attachment.Open())
            await attachmentStream.CopyToAsync(ms);
        ms.Position = 0;

        // HACK: Catch MongoWriteException because confluence export maybe include two different attachments for one page with same name
        await Try.DoAsync(() => _attachmentManager.Upload(actualHomePage, ms, name, updatedBy), e => e is MongoWriteException);
        GC.Collect();
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
        var allowedCollections = new string[] { "children", "bodyContents", "attachments" };

        var pages = doc.Descendants().Where(x => x.Name == "object" && x.Attribute("class")?.Value == "Page");
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
        var currentPages = pages.Where(x => x["contentStatus"]?.ToString() == "current");
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
        currentPages = null;
        GC.Collect();
        return latestPages.Values.ToList();
    }

    private static IDictionary<string, string> ParseContents(XDocument doc)
    {
        var bodys = doc.Descendants().Where(x => x.Name == "object" && x.Attribute("class")?.Value == "BodyContent");
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

    private static IDictionary<string, string> ParseAttachments(XDocument doc)
    {
        var attachments = doc.Descendants().Where(x => x.Name == "object" && x.Attribute("class")?.Value == "Attachment");
        var attachesDict = new Dictionary<string, string>();

        foreach (var attachment in attachments)
        {
            var attachmentElements = attachment.Elements();
            var id = attachmentElements.FirstOrDefault(x => x.Name.ToString() == "id")?.Value;
            var name = attachmentElements.FirstOrDefault(x => x.Name.ToString() == "property" && x.Attribute("name")?.Value == "title")?.Value;
            if (string.IsNullOrEmpty(id) && string.IsNullOrEmpty(name))
                continue;
            attachesDict.Add(id, name);
        }
        return attachesDict;
    }

    private static Task<string> ReadEntitiesFromZip(ZipArchive archive)
    {
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            if (entry.FullName.Equals("entities.xml", StringComparison.OrdinalIgnoreCase))
            {
                using var reader = new StreamReader(entry.Open());
                return reader.ReadToEndAsync();
            }
        }

        return null;
    }
}
