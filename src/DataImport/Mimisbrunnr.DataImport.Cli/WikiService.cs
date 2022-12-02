using Mimisbrunnr.Integration.Client;
using Mimisbrunnr.Integration.Wiki;

namespace Mimisbrunnr.DataImport.Cli;

internal class WikiService : IWikiService
{
    private readonly MimisbrunnrClient _client;

    public WikiService(string host, string accessToken)
    {
        if (string.IsNullOrEmpty(host))
            throw new ArgumentNullException(nameof(host));
        if (string.IsNullOrEmpty(accessToken))
            throw new ArgumentNullException(nameof(accessToken));

        var configuration = new MimisbrunnrClientConfiguration()
        {
            AccessToken = accessToken,
            Host = host
        };
        _client = new MimisbrunnrClient(new HttpClient(), configuration);
    }

    public Task<PageModel> CreatePage(PageCreateModel createModel)
    {
        return _client.Pages.Create(createModel);
    }

    public Task<PageModel> GetPageById(string id)
    {
        return _client.Pages.GetById(id);
    }

    public Task<SpaceModel> GetSpaceByKey(string key)
    {
        return _client.Spaces.GetByKey(key);
    }

    public Task UpdatePage(string pageId, PageUpdateModel updateModel)
    {
        return _client.Pages.Update(pageId, updateModel);
    }

    public Task UploadAttachment(string pageId, Stream content, string name)
    {
        return _client.Attachments.Upload(pageId, content, name);
    }

    public Task<SpaceModel> CreateSpace(SpaceCreateModel spaceCreateModel)
    {
        return _client.Spaces.Create(spaceCreateModel);
    }
}