using System.Net;
using System.Net.Http.Json;
using Mimisbrunnr.Integration.Wiki;

namespace Mimisbrunnr.Integration.Client;

public sealed class PageService
{
    private const string ApiPath = "/api/page";

    private readonly HttpClient _httpClient;

    public PageService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PageModel> GetById(string pageId, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.GetAsync($"{ApiPath}/{pageId}", cancellationToken);
        if (request.StatusCode == HttpStatusCode.OK)
            return await request.Content.ReadFromJsonAsync<PageModel>(cancellationToken: cancellationToken);
        if (request.StatusCode == HttpStatusCode.NotFound)
            throw new NotFoundException();

        request.EnsureSuccessStatusCode();
        return null;
    }

    public async Task<PageTreeModel> GetPageTreeModel(string pageId, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.GetAsync($"{ApiPath}/{pageId}/tree", cancellationToken);
        if (request.StatusCode == HttpStatusCode.OK)
            return await request.Content.ReadFromJsonAsync<PageTreeModel>(cancellationToken: cancellationToken);
        if (request.StatusCode == HttpStatusCode.NotFound)
            throw new NotFoundException();

        request.EnsureSuccessStatusCode();
        return null;
    }

    public async Task<PageModel> Create(PageCreateModel createModel, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.PostAsJsonAsync($"{ApiPath}", createModel, cancellationToken: cancellationToken);
        if (request.StatusCode == HttpStatusCode.OK)
            return await request.Content.ReadFromJsonAsync<PageModel>(cancellationToken: cancellationToken);

        request.EnsureSuccessStatusCode();
        return null;
    }

    public async Task Update(string pageId, PageUpdateModel updateModel, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.PutAsJsonAsync($"{ApiPath}/{pageId}", updateModel, cancellationToken: cancellationToken);
        if (request.StatusCode == HttpStatusCode.OK)
            return;
        if (request.StatusCode == HttpStatusCode.NotFound)
            throw new NotFoundException();

        request.EnsureSuccessStatusCode();
    }

    public async Task Delete(string pageId, bool recursively = false, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.DeleteAsync($"{ApiPath}/{pageId}?recursively={recursively}", cancellationToken);
        if (request.StatusCode == HttpStatusCode.OK)
            return;
        if (request.StatusCode == HttpStatusCode.NotFound)
            throw new NotFoundException();

        request.EnsureSuccessStatusCode();
    }

    public async Task<PageModel> Copy(string sourcePageId, string destinationParentPageId, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.PostAsync($"{ApiPath}/copy/{sourcePageId}/{destinationParentPageId}", null, cancellationToken);
        if (request.StatusCode == HttpStatusCode.OK)
            return await request.Content.ReadFromJsonAsync<PageModel>(cancellationToken: cancellationToken);

        request.EnsureSuccessStatusCode();
        return null;
    }

    public async Task<PageModel> Move(string sourcePageId, string destinationParentPageId, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.PostAsync($"{ApiPath}/move/{sourcePageId}/{destinationParentPageId}", null, cancellationToken);
        if (request.StatusCode == HttpStatusCode.OK)
            return await request.Content.ReadFromJsonAsync<PageModel>(cancellationToken: cancellationToken);

        request.EnsureSuccessStatusCode();
        return null;
    }

}