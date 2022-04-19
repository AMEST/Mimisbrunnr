using Microsoft.Extensions.Caching.Distributed;
using Mimisbrunnr.Users.Services;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Users;

internal class UserManager : IUserManager
{
    private readonly TimeSpan _defaultCacheTime = TimeSpan.FromMinutes(10);
    private readonly IRepository<User> _userRepository;
    private readonly IDistributedCache _distributedCache;

    public UserManager(IRepository<User> userRepository, IDistributedCache distributedCache)
    {
        _userRepository = userRepository;
        _distributedCache = distributedCache;
    }
    
    public async Task<User> GetByEmail(string email)
    {
        var user = await _distributedCache.GetAsync<User>(GetUserCacheKey(email));
        if( user is not null)
            return user;

        user = await _userRepository.GetAll().FirstOrDefaultAsync(x => x.Email == email.ToLower());
        if(user is null)
            return user;

        await _distributedCache.SetAsync(GetUserCacheKey(email), user, new DistributedCacheEntryOptions(){
            AbsoluteExpirationRelativeToNow = _defaultCacheTime
        });
        return user;
    }

    public async Task Add(string email, string name, string avatarUrl, UserRole role)
    {
        var user = new User()
        {
            Email = email.ToLower(),
            Name = name,
            AvatarUrl = avatarUrl,
            Role = role
        };
        await _userRepository.Create(user);
        await _distributedCache.SetAsync(GetUserCacheKey(email), user, new DistributedCacheEntryOptions(){
            AbsoluteExpirationRelativeToNow = _defaultCacheTime
        });
    }

    public async Task Disable(User user)
    {
        user.Enable = false;
        await _userRepository.Update(user);
        await _distributedCache.RemoveAsync(GetUserCacheKey(user.Email));
    }

    public async Task Enable(User user)
    {
        user.Enable = true;
        await _userRepository.Update(user);
        await _distributedCache.RemoveAsync(GetUserCacheKey(user.Email));
    }

    public async Task ChangeRole(User user, UserRole role)
    {
        user.Role = role;
        await _userRepository.Update(user);
        await _distributedCache.RemoveAsync(GetUserCacheKey(user.Email));
    }

    private static string GetUserCacheKey(string email) => $"user_cache_{email.ToLower()}";
}