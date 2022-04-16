using Microsoft.Extensions.Caching.Distributed;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Infrastructure.Contracts;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Web.Infrastructure;

internal class ApplicationConfigurationService : IApplicationConfigurationService
{
    private const string ConfigurationCacheKey = "ApplicationConfigurationCache";
    private readonly TimeSpan _defaultCacheTime = TimeSpan.FromMinutes(10);
    private readonly IRepository<ApplicationConfiguration> _repository;
    private readonly IUserManager _userManager;
    private readonly IDistributedCache _distributedCache;

    public ApplicationConfigurationService(IRepository<ApplicationConfiguration> repository, 
        IUserManager userManager,
        IDistributedCache distributedCache)
    {
        _repository = repository;
        _userManager = userManager;
        _distributedCache = distributedCache;
    }
    
    public async Task Initialize(ApplicationConfiguration configuration, User initializedBy)
    {
        var isInitialized = await IsInitialized();
        if (isInitialized)
            throw new InvalidOperationException("Mimisbrunner alredy initialized");
        
        await _repository.Create(configuration);
        await _userManager.ChangeRole(initializedBy, UserRole.Admin);
    }

    public async Task<bool> IsInitialized()
    {
        var configuration = await Get();
        return configuration is not null;
    }

    public async Task<ApplicationConfiguration> Get()
    {
        var configuration = await _distributedCache.GetAsync<ApplicationConfiguration>(ConfigurationCacheKey);
        if(configuration is not null)
            return configuration;
        
        configuration = _repository.GetAll().SingleOrDefault();
        if(configuration == null)
            return null;

        await _distributedCache.SetAsync(ConfigurationCacheKey, configuration, new DistributedCacheEntryOptions(){ AbsoluteExpirationRelativeToNow = _defaultCacheTime});
        return configuration;
    }

    public async Task Configure(ApplicationConfiguration configuration)
    {
        var isInitialized = await IsInitialized();
        if (!isInitialized)
            throw new InvalidOperationException("Mimisbrunner not initialized");
        await _repository.Update(configuration);
        await _distributedCache.RemoveAsync(ConfigurationCacheKey);
    }
}