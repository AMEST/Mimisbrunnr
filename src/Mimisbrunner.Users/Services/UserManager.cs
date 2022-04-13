using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunner.Users;

internal class UserManager : IUserManager
{
    private readonly IRepository<User> _userRepository;

    public UserManager(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }
    
    public Task<User> GetByEmail(string email)
    {
        return Task.FromResult(_userRepository.GetAll().FirstOrDefault(x => x.Email == email.ToLower()));
    }

    public Task Add(string email, string name, string avatarUrl, UserRole role)
    {
        return _userRepository.Create(new User()
        {
            Email = email.ToLower(),
            Name = name,
            AvatarUrl = avatarUrl,
            Role = role
        });
    }

    public Task Disable(User user)
    {
        user.Enable = false;
        return _userRepository.Update(user);
    }

    public Task Enable(User user)
    {
        user.Enable = true;
        return _userRepository.Update(user);
    }

    public Task ChangeRole(User user, UserRole role)
    {
        user.Role = role;
        return _userRepository.Update(user);
    }
}