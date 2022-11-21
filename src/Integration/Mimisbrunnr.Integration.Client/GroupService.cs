using Mimisbrunnr.Integration.Group;
using Mimisbrunnr.Integration.User;

namespace Mimisbrunnr.Integration.Client;

public sealed class GroupService
{
    private readonly HttpClient _httpClient;

    public GroupService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

     Task<IEnumerable<GroupModel>> GetAll(GroupFilterModel filter) => default;
     
     Task<GroupModel> Get(string name) => default;

     Task<GroupModel> Create(GroupCreateModel createModel) => default;

     Task Remove(string name) => default;

     Task Update(string name, GroupUpdateModel model) => default;

     Task<IEnumerable<UserModel>> GetUsers(string name) => default;

     Task AddUserToGroup(string name, UserModel user) => default;

     Task RemoveUserFromGroup(string name, UserModel user) => default;
}