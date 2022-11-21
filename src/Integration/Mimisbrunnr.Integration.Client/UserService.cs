using Mimisbrunnr.Integration.Group;
using Mimisbrunnr.Integration.User;

namespace Mimisbrunnr.Integration.Client;

public sealed class UserService
{
    private readonly HttpClient _httpClient;

    public UserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    Task<IEnumerable<UserModel>> GetUsers(int? offset = null) => default;

    Task<UserViewModel> GetCurrent() => default;

    Task<IEnumerable<GroupModel>> GetUserGroups(string email) => default;

    Task<UserProfileModel> GetByEmail(string email) => default;

    Task Disable(string email) => default;

    Task Enable(string email) => default;

    Task Promote(string email) => default;
    
    Task Demote(string email) => default;
}