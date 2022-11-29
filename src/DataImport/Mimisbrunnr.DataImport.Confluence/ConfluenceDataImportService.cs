using System.IO.Compression;
using System.Xml.Linq;
using Mimisbrunnr.Integration.Wiki;
using ReverseMarkdown;
using Mimisbrunnr.DataImport;
using Skidbladnir.Utility.Common;
using Mimisbrunnr.DataImport.Contracts;
using Microsoft.Extensions.Logging;

namespace Mimisbrunnr.Web.Host.Services;

public class ConfluenceDataImportService : IDataImportService
{
    private readonly Converter _markdownConverter;
    private readonly IWikiService _wikiService;
    private readonly ILogger<ConfluenceDataImportService> _logger;

    public ConfluenceDataImportService(
        IWikiService wikiService,
        ILogger<ConfluenceDataImportService> logger
    )
    {
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
        _wikiService = wikiService;
        _logger = logger;
    }

    public async Task ImportSpace(SpaceModel space, Stream importStream)
    {
        if (space is null)
            throw new SpaceNotFoundException();

        _logger.LogInformation("Starting import space to {SpaceKey}", space.Key);

        using (var archive = new ZipArchive(importStream))
        {
            _logger.LogDebug("Open zip archive with exported space");
            var entities = await ReadEntitiesFromZip(archive).ConfigureAwait(false);
            var entitiesDocument = XDocument.Parse(entities);
            entities = null;

            _logger.LogDebug("Parse Entities.xmls");
            var attachments = ParseAttachments(entitiesDocument);
            var contents = ParseContents(entitiesDocument);
            var pages = ParsePages(entitiesDocument);
            entitiesDocument = null;


            _logger.LogDebug("Filter pages");
            pages = FilterMostRecent(pages);

            var spaceHomePage = ToPage(pages.FirstOrDefault(x => !x.ContainsKey("parent") && x.ContainsKey("children")), space.Key, contents);

            var flatPagesContracts = pages.Where(x => x.ContainsKey("parent")).Select(x => ToPage(x, space.Key, contents));
            var pageTree = ToPageTreeModel(flatPagesContracts, spaceHomePage);
            flatPagesContracts = null;
            contents.Clear();

            var homePageAttachmentIds = pages.Where(x => x.ContainsKey("parent")).FirstOrDefault(x => x.ContainsKey("id") && ((string)x["id"]) == spaceHomePage.Id && x.ContainsKey("attachments"))
                ?.Where(x => x.Key == "attachments")?.Select(x => x.Value)?.FirstOrDefault() as string[] ?? Array.Empty<string>();
            _logger.LogDebug("Update Homepage {PageId}", space.HomePageId);
            await UpdateHomePage(space.HomePageId, spaceHomePage, 
                attachments.Where(x => homePageAttachmentIds.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value), archive)
                    .ConfigureAwait(false);

            foreach (var child in pageTree.Childs)
                await CreatePageFromTree(child, space.Key, space.HomePageId, pages, attachments, archive)  
                    .ConfigureAwait(false);
            pages.Clear();
            pageTree = null;
            attachments.Clear();
        }
        _logger.LogInformation("Import finished");
    }

    private async Task UpdateHomePage(string homePageId, PageModel newHomePage, IDictionary<string, string> attachments, ZipArchive archive)
    {
        var pageUpdateModel = new PageUpdateModel()
        {
            Content = newHomePage.Content,
            Name = newHomePage.Name
        };
        if (attachments.Keys.Count == 0)
        {
            await _wikiService.UpdatePage(homePageId, pageUpdateModel)
                .ConfigureAwait(false);
            return;
        }

        pageUpdateModel.Content = ConfluenceContentProcessing.PostProcess(newHomePage.Content, homePageId);
        await _wikiService.UpdatePage(homePageId, pageUpdateModel);

        foreach (var (id, name) in attachments)
            await UploadAttachment(homePageId, newHomePage.Id, id, name, archive)
                .ConfigureAwait(false);
    }

    private async Task CreatePageFromTree(PageTreeModel pageTreeModel, string spaceKey, string parentPageId, IList<IDictionary<string, object>> rawPages, IDictionary<string, string> attachments, ZipArchive archive)
    {
        _logger.LogDebug("Creating page {PageName}", pageTreeModel.Page.Name);
        var page = await _wikiService.CreatePage(new PageCreateModel(){
            ParentPageId = parentPageId,
            Content = pageTreeModel.Page.Content,
            Name = pageTreeModel.Page.Name,
            SpaceKey = spaceKey
        }).ConfigureAwait(false);

        var pageAttachmentsIds = rawPages.Where(x => x.ContainsKey("parent")).FirstOrDefault(x => x.ContainsKey("id") && ((string)x["id"]) == pageTreeModel.Page.Id && x.ContainsKey("attachments"))
            ?.Where(x => x.Key == "attachments")?.Select(x => x.Value)?.FirstOrDefault() as string[] ?? Array.Empty<string>();
        var pageAttachments = attachments.Where(x => pageAttachmentsIds.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
        if (pageAttachments.Keys.Count != 0)
        {
            await _wikiService.UpdatePage(page.Id, new PageUpdateModel(){
                Content = ConfluenceContentProcessing.PostProcess(page.Content, page.Id),
                Name = page.Name
            }).ConfigureAwait(false);
            foreach (var (id, name) in pageAttachments)
                await UploadAttachment(page.Id, pageTreeModel.Page.Id, id, name, archive)
                    .ConfigureAwait(false);
        }
        foreach (var child in pageTreeModel.Childs)
            await CreatePageFromTree(child, spaceKey, page.Id, rawPages, attachments, archive)
                .ConfigureAwait(false);

    }

    private async Task UploadAttachment(string pageId, string importPageId, string id, string name, ZipArchive archive)
    {
        var attachment = archive.Entries.Where(x => x.FullName.StartsWith($"attachments/{importPageId}/{id}"))?.OrderByDescending(x => x.FullName)?.FirstOrDefault();
        if (attachment == null) return;

        _logger.LogDebug("Upload attachment {AttachmentName} to page {PageId}", name, pageId);
        // HACK: Catch (Temporary Any) Exception because confluence export maybe include two different attachments for one page with same name
        using (var attachmentStream = attachment.Open())
            await Try.DoAsync(() => _wikiService.UploadAttachment(pageId, attachmentStream, name), e => true)
                .ConfigureAwait(false);
    }

#region Mappings
    private ExtendedPageModel ToPage(IDictionary<string, object> page, string spaceKey, IDictionary<string, string> contents)
    {
        var body = "Empty page";
        if (page.ContainsKey("bodyContents"))
        {
            var contentId = ((string[])page["bodyContents"]).First();
            var content = contents[contentId];
            body = _markdownConverter.Convert(ConfluenceContentProcessing.Process(content));
        }

        return new ExtendedPageModel()
        {
            Id = page.ContainsKey("id") ? page["id"]?.ToString() : string.Empty,
            Name = page.ContainsKey("title") ? page["title"]?.ToString() : string.Empty,
            SpaceKey = spaceKey,
            Content = body,
            ParentId = page.ContainsKey("parent") ? page["parent"]?.ToString() : null
        };
    }

    private static PageTreeModel ToPageTreeModel(IEnumerable<ExtendedPageModel> childs, PageModel rootPage)
    {
        var pageTree = new PageTreeModel
        {
            Page = rootPage
        };
        var childsPages = childs.Where(x => x.ParentId == rootPage?.Id).Select(x => ToPageTreeModel(childs, x)).ToList();
        pageTree.Childs = childsPages;
        return pageTree;
    }
#endregion
#region  RawConfluenceXmlParse
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

    private static async Task<string> ReadEntitiesFromZip(ZipArchive archive)
    {
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            if (entry.FullName.Equals("entities.xml", StringComparison.OrdinalIgnoreCase))
            {
                using var reader = new StreamReader(entry.Open());
                return await reader.ReadToEndAsync().ConfigureAwait(false);
            }
        }

        return null;
    }
#endregion
}
