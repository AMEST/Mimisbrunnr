using System.Net;
using System.Net.Http.Json;
using Mimisbrunnr.Integration.Group;
using Mimisbrunnr.Integration.User;

namespace Mimisbrunnr.Integration.Client;

public sealed class UserService
{
    private const string ApiPath = "/api/user";

    private readonly HttpClient _httpClient;

    public UserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<UserModel>> GetAll(int? offset = null, CancellationToken cancellationToken = default)
    {
        var offsetParameter = offset.HasValue ? $"?offset={offset}" : "";
        var request = await _httpClient.GetAsync($"{ApiPath}{offsetParameter}", cancellationToken);
        if (request.StatusCode == HttpStatusCode.OK)
            return await request.Content.ReadFromJsonAsync<IEnumerable<UserModel>>(cancellationToken: cancellationToken);

        request.EnsureSuccessStatusCode();
        return null;
    }

    public async Task<UserViewModel> GetCurrent(CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.GetAsync($"{ApiPath}/current", cancellationToken);
        if (request.StatusCode == HttpStatusCode.OK)
            return await request.Content.ReadFromJsonAsync<UserViewModel>(cancellationToken: cancellationToken);
        if (request.StatusCode == HttpStatusCode.NotFound)
            return null;

        request.EnsureSuccessStatusCode();
        return null;
    }

    public async Task<IEnumerable<GroupModel>> GetUserGroups(string email, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.GetAsync($"{ApiPath}/{email}/groups", cancellationToken);
        if (request.StatusCode == HttpStatusCode.OK)
            return await request.Content.ReadFromJsonAsync<IEnumerable<GroupModel>>(cancellationToken: cancellationToken);

        request.EnsureSuccessStatusCode();
        return null;
    }

    public async Task<UserProfileModel> GetByEmail(string email, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.GetAsync($"{ApiPath}/{email}", cancellationToken);
        if (request.StatusCode == HttpStatusCode.OK)
            return await request.Content.ReadFromJsonAsync<UserProfileModel>(cancellationToken: cancellationToken);
        if (request.StatusCode == HttpStatusCode.NotFound)
            return null;

        request.EnsureSuccessStatusCode();
        return null;
    }

    public async Task Disable(string email, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.PostAsync($"{ApiPath}/{email}/disable", null, cancellationToken);
        if (request.StatusCode == HttpStatusCode.OK)
            return;

        request.EnsureSuccessStatusCode();
    }

    public async Task Enable(string email, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.PostAsync($"{ApiPath}/{email}/enable", null, cancellationToken);
        if (request.StatusCode == HttpStatusCode.OK)
            return;

        request.EnsureSuccessStatusCode();
    }

    public async Task Promote(string email, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.PostAsync($"{ApiPath}/{email}/promote", null, cancellationToken);
        if (request.StatusCode == HttpStatusCode.OK)
            return;

        request.EnsureSuccessStatusCode();
    }

    public async Task Demote(string email, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.PostAsync($"{ApiPath}/{email}/demote", null, cancellationToken);
        if (request.StatusCode == HttpStatusCode.OK)
            return;

        request.EnsureSuccessStatusCode();
    }
}