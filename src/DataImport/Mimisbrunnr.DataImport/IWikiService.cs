using Mimisbrunnr.Integration.Wiki;

namespace Mimisbrunnr.DataImport;

public interface IWikiService
{
    Task<SpaceModel> GetSpaceByKey(string key);

    Task<PageModel> GetPageById(string id);

    Task<PageModel> CreatePage(PageCreateModel createModel);

    Task UpdatePage(string pageId, PageUpdateModel updateModel);

    Task UploadAttachment(string pageId, Stream content, string name);
}