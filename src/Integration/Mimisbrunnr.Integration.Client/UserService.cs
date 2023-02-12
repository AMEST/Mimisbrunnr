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
        var request = await _httpClient.GetAsync($"{ApiPath}{offsetParameter}", cancellationToken)
            .ConfigureAwait(false);
        if (request.StatusCode == HttpStatusCode.OK)
            return await request.Content.ReadFromJsonAsync<IEnumerable<UserModel>>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

        request.EnsureSuccessStatusCode();
        return Array.Empty<UserModel>();
    }

    public async Task<UserViewModel> GetCurrent(CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.GetAsync($"{ApiPath}/current", cancellationToken)
            .ConfigureAwait(false);
        if (request.StatusCode == HttpStatusCode.OK)
            return await request.Content.ReadFromJsonAsync<UserViewModel>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        if (request.StatusCode == HttpStatusCode.NotFound)
            return null;

        request.EnsureSuccessStatusCode();
        return null;
    }

    public async Task<IEnumerable<GroupModel>> GetUserGroups(string email, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.GetAsync($"{ApiPath}/{email}/groups", cancellationToken)
            .ConfigureAwait(false);
        if (request.StatusCode == HttpStatusCode.OK)
            return await request.Content.ReadFromJsonAsync<IEnumerable<GroupModel>>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

        request.EnsureSuccessStatusCode();
        return null;
    }

    public async Task<UserProfileModel> GetByEmail(string email, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.GetAsync($"{ApiPath}/{email}", cancellationToken)
            .ConfigureAwait(false);
        if (request.StatusCode == HttpStatusCode.OK)
            return await request.Content.ReadFromJsonAsync<UserProfileModel>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        if (request.StatusCode == HttpStatusCode.NotFound)
            return null;

        request.EnsureSuccessStatusCode();
        return null;
    }

    public async Task<UserModel> Create(UserCreateModel model, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.PostAsJsonAsync($"{ApiPath}", model, cancellationToken)
            .ConfigureAwait(false);
        if (request.StatusCode == HttpStatusCode.OK)
            return await request.Content.ReadFromJsonAsync<UserModel>(cancellationToken: cancellationToken);

        request.EnsureSuccessStatusCode();
        return null;
    }

    public async Task UpdateProfileInfo(string email, UserProfileUpdateModel model, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.PutAsJsonAsync($"{ApiPath}/{email}", model, cancellationToken)
            .ConfigureAwait(false);
        if (request.StatusCode == HttpStatusCode.OK)
            return;

        request.EnsureSuccessStatusCode();
    }

    public async Task Disable(string email, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.PostAsync($"{ApiPath}/{email}/disable", null, cancellationToken)
            .ConfigureAwait(false);
        if (request.StatusCode == HttpStatusCode.OK)
            return;

        request.EnsureSuccessStatusCode();
    }

    public async Task Enable(string email, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.PostAsync($"{ApiPath}/{email}/enable", null, cancellationToken)
            .ConfigureAwait(false);
        if (request.StatusCode == HttpStatusCode.OK)
            return;

        request.EnsureSuccessStatusCode();
    }

    public async Task Promote(string email, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.PostAsync($"{ApiPath}/{email}/promote", null, cancellationToken)
            .ConfigureAwait(false);
        if (request.StatusCode == HttpStatusCode.OK)
            return;

        request.EnsureSuccessStatusCode();
    }

    public async Task Demote(string email, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.PostAsync($"{ApiPath}/{email}/demote", null, cancellationToken)
            .ConfigureAwait(false);
        if (request.StatusCode == HttpStatusCode.OK)
            return;

        request.EnsureSuccessStatusCode();
    }
}