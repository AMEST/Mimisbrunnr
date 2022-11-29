using System.Net;
using System.Net.Http.Json;
using Mimisbrunnr.Integration.Group;
using Mimisbrunnr.Integration.User;

namespace Mimisbrunnr.Integration.Client;

public sealed class GroupService
{
    private const string ApiPath = "/api/group";
    private readonly HttpClient _httpClient;

    public GroupService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<GroupModel>> GetAll(GroupFilterModel filter = null, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.GetAsync($"{ApiPath}?{filter?.GetQueryString()}", cancellationToken)
            .ConfigureAwait(false);
        if (request.StatusCode == HttpStatusCode.OK)
            return await request.Content.ReadFromJsonAsync<IEnumerable<GroupModel>>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

        request.EnsureSuccessStatusCode();
        return Array.Empty<GroupModel>();
    }

    public async Task<GroupModel> Get(string name, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.GetAsync($"{ApiPath}/{name}", cancellationToken)
            .ConfigureAwait(false);
        if (request.StatusCode == HttpStatusCode.OK)
            return await request.Content.ReadFromJsonAsync<GroupModel>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        if (request.StatusCode == HttpStatusCode.NotFound)
            throw new NotFoundException($"Group with name: {name} not found");

        request.EnsureSuccessStatusCode();
        return null;
    }

    public async Task<GroupModel> Create(GroupCreateModel createModel, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.PostAsJsonAsync($"{ApiPath}", createModel, cancellationToken)
            .ConfigureAwait(false);
        if (request.StatusCode == HttpStatusCode.OK)
            return await request.Content.ReadFromJsonAsync<GroupModel>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

        request.EnsureSuccessStatusCode();
        return null;
    }

    public async Task Remove(string name, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.DeleteAsync($"{ApiPath}/{name}", cancellationToken)
            .ConfigureAwait(false);
        if (request.StatusCode == HttpStatusCode.OK)
            return;

        request.EnsureSuccessStatusCode();
    }

    public async Task Update(string name, GroupUpdateModel model, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.PutAsJsonAsync($"{ApiPath}/{name}", model, cancellationToken)
            .ConfigureAwait(false);
        if (request.StatusCode == HttpStatusCode.OK)
            return;

        request.EnsureSuccessStatusCode();
    }

    public async Task<IEnumerable<UserModel>> GetUsers(string name, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.GetAsync($"{ApiPath}/{name}/users", cancellationToken)
            .ConfigureAwait(false);
        if (request.StatusCode == HttpStatusCode.OK)
            return await request.Content.ReadFromJsonAsync<IEnumerable<UserModel>>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        if (request.StatusCode == HttpStatusCode.NotFound)
            throw new NotFoundException($"Group with name: {name} not found");

        request.EnsureSuccessStatusCode();
        return Array.Empty<UserModel>();
    }

    public async Task AddUserToGroup(string name, UserModel user, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.PostAsync($"{ApiPath}/{name}/{user.Email}", null, cancellationToken)
            .ConfigureAwait(false);
        if (request.StatusCode == HttpStatusCode.OK)
            return;

        request.EnsureSuccessStatusCode();
    }

    public async Task RemoveUserFromGroup(string name, UserModel user, CancellationToken cancellationToken = default)
    {
        var request = await _httpClient.DeleteAsync($"{ApiPath}/{name}/{user.Email}", cancellationToken)
            .ConfigureAwait(false);
        if (request.StatusCode == HttpStatusCode.OK)
            return;

        request.EnsureSuccessStatusCode();
    }
}