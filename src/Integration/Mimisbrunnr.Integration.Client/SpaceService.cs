using System.Net;
using System.Net.Http.Json;
using Mimisbrunnr.Integration.Wiki;
namespace Mimisbrunnr.Integration.Client;

public sealed class SpaceService
{
    private const string ApiPath = "/api/space";

    private readonly HttpClient _httpClient;

    public SpaceService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<SpaceModel[]> GetAll(CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.GetAsync($"{ApiPath}", cancellationToken);
        if (request.StatusCode == HttpStatusCode.OK)
            return await request.Content.ReadFromJsonAsync<SpaceModel[]>(cancellationToken: cancellationToken);

        request.EnsureSuccessStatusCode();
        return null;
    }

    public async Task<SpaceModel> GetByKey(string key, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.GetAsync($"{ApiPath}/{key}", cancellationToken);
        if (request.StatusCode == HttpStatusCode.OK)
            return await request.Content.ReadFromJsonAsync<SpaceModel>(cancellationToken: cancellationToken);
        if (request.StatusCode == HttpStatusCode.NotFound)
            throw new NotFoundException();

        request.EnsureSuccessStatusCode();
        return null;
    }

    public async Task<UserPermissionModel> GetPermission(string key, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.GetAsync($"{ApiPath}/{key}/permission", cancellationToken);
        if (request.StatusCode == HttpStatusCode.OK)
            return await request.Content.ReadFromJsonAsync<UserPermissionModel>(cancellationToken: cancellationToken);
        if (request.StatusCode == HttpStatusCode.NotFound)
            throw new NotFoundException();

        request.EnsureSuccessStatusCode();
        return null;
    }

    public async Task<SpacePermissionModel[]> GetSpacePermissions(string key, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.GetAsync($"{ApiPath}/{key}/permissions", cancellationToken);
        if (request.StatusCode == HttpStatusCode.OK)
            return await request.Content.ReadFromJsonAsync<SpacePermissionModel[]>(cancellationToken: cancellationToken);
        if (request.StatusCode == HttpStatusCode.NotFound)
            throw new NotFoundException();

        request.EnsureSuccessStatusCode();
        return null;
    }

    public async Task<SpacePermissionModel> AddPermission(string key, SpacePermissionModel model, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.PostAsJsonAsync($"{ApiPath}/{key}/permissions", model, cancellationToken);
        if (request.StatusCode == HttpStatusCode.OK)
            return await request.Content.ReadFromJsonAsync<SpacePermissionModel>(cancellationToken: cancellationToken);
        if (request.StatusCode == HttpStatusCode.NotFound)
            throw new NotFoundException();

        request.EnsureSuccessStatusCode();
        return null;
    }

    public async Task UpdatePermission(string key, SpacePermissionModel model, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.PutAsJsonAsync($"{ApiPath}/{key}/permissions", model, cancellationToken);
        if (request.StatusCode == HttpStatusCode.OK)
            return;
        if (request.StatusCode == HttpStatusCode.NotFound)
            throw new NotFoundException();

        request.EnsureSuccessStatusCode();
    }

    public async Task RemovePermission(string key, SpacePermissionModel model, CancellationToken cancellationToken = default)
    {
        var requestMessage = new HttpRequestMessage()
        {
            Method = HttpMethod.Delete,
            Content = JsonContent.Create(model),
            RequestUri = new Uri(_httpClient.BaseAddress, $"{ApiPath}/{key}/permissions")
        };

        var request = await _httpClient.SendAsync(requestMessage, cancellationToken);
        if (request.StatusCode == HttpStatusCode.OK)
            return;

        if (request.StatusCode == HttpStatusCode.NotFound)
            throw new NotFoundException();

        request.EnsureSuccessStatusCode();
    }

    public async Task<SpaceModel> Create(SpaceCreateModel model, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.PostAsJsonAsync($"{ApiPath}", model, cancellationToken);
        if (request.StatusCode == HttpStatusCode.OK)
            return await request.Content.ReadFromJsonAsync<SpaceModel>(cancellationToken: cancellationToken);
        if (request.StatusCode == HttpStatusCode.NotFound)
            throw new NotFoundException();

        request.EnsureSuccessStatusCode();
        return null;
    }

    public async Task Update(string key, SpaceUpdateModel model, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.PutAsJsonAsync($"{ApiPath}/{key}", model, cancellationToken);
        if (request.StatusCode == HttpStatusCode.OK)
            return;
        if (request.StatusCode == HttpStatusCode.NotFound)
            throw new NotFoundException();

        request.EnsureSuccessStatusCode();
    }

    public async Task Archive(string key, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.PostAsync($"{ApiPath}/{key}/archive", null, cancellationToken);
        if (request.StatusCode == HttpStatusCode.OK)
            return;
        if (request.StatusCode == HttpStatusCode.NotFound)
            throw new NotFoundException();

        request.EnsureSuccessStatusCode();
    }

    public async Task UnArchive(string key, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.PostAsync($"{ApiPath}/{key}/unarchive", null, cancellationToken);
        if (request.StatusCode == HttpStatusCode.OK)
            return;
        if (request.StatusCode == HttpStatusCode.NotFound)
            throw new NotFoundException();

        request.EnsureSuccessStatusCode();
    }

    public async Task Remove(string key, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.DeleteAsync($"{ApiPath}/{key}", cancellationToken);
        if (request.StatusCode == HttpStatusCode.OK)
            return;
        if (request.StatusCode == HttpStatusCode.NotFound)
            throw new NotFoundException();

        request.EnsureSuccessStatusCode();
    }
}