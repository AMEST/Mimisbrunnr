namespace Mimisbrunnr.Users;

public interface IUserManager
{
    Task<User[]> GetUsers(int? offset = null);
    
    Task<User> GetByEmail(string email);

    Task<User> GetById(string id);

    Task Add(string email, string name, string avatarUrl, UserRole role);

    Task Disable(User user);

    Task Enable(User user);

    Task ChangeRole(User user, UserRole role);
}