using Mimisbrunnr.Users.Services;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Users;

internal class UserManager : IUserManager, IUserSearcher
{
    private const int DefaultMaxUserGet = 15;
    private readonly IRepository<User> _userRepository;

    public UserManager(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public Task<User[]> GetUsers(int? offset = null)
    {
        if (offset is null)
            return _userRepository.GetAll().ToArrayAsync();

        return _userRepository.GetAll()
            .Skip(offset.Value)
            .Take(DefaultMaxUserGet)
            .ToArrayAsync();
    }

    public async Task<User> GetByEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return null;

        var user = await _userRepository.GetAll().FirstOrDefaultAsync(x => x.Email == email.ToLower());
        return user;
    }

    public async Task<User> GetById(string id)
    {
        var user =  await _userRepository.GetAll().FirstOrDefaultAsync(x => x.Id == id.ToLower());
        return user;
    }

    public async Task<User> Add(string email, string name, string avatarUrl, UserRole role)
    {
        var user = new User()
        {
            Email = email.ToLower(),
            Name = name,
            AvatarUrl = avatarUrl,
            Role = role
        };
        await _userRepository.Create(user);
        return user;
    }

    public async Task Disable(User user)
    {
        user.Enable = false;
        await _userRepository.Update(user);
    }

    public async Task Enable(User user)
    {
        user.Enable = true;
        await _userRepository.Update(user);
    }

    public async Task ChangeRole(User user, UserRole role)
    {
        user.Role = role;
        await _userRepository.Update(user);
    }

    public async Task UpdateUserInfo(User user)
    {
        await _userRepository.Update(user);
    }

    public async Task<IEnumerable<User>> Search(string text)
    {
        var textLower = text.ToLower();
        var users = await _userRepository.GetAll()
            .Where(x => x.Name.ToLower().Contains(textLower)
                || x.Email.ToLower().Contains(textLower))
            .Take(20)
            .ToArrayAsync();
        return users;
    }
}