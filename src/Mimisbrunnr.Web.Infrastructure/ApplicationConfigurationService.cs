using Microsoft.Extensions.Caching.Memory;
using Mimisbrunner.Users;
using Mimisbrunnr.Web.Infrastructure.Contracts;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Web.Infrastructure;

internal class ApplicationConfigurationService : IApplicationConfigurationService
{
    private const string ConfigurationCacheKey = "ApplicationConfigurationCache";
    private readonly IRepository<ApplicationConfiguration> _repository;
    private readonly IUserManager _userManager;
    private readonly IMemoryCache _memoryCache;

    public ApplicationConfigurationService(IRepository<ApplicationConfiguration> repository, 
        IUserManager userManager,
        IMemoryCache memoryCache)
    {
        _repository = repository;
        _userManager = userManager;
        _memoryCache = memoryCache;
    }
    
    public async Task Initialize(ApplicationConfiguration configuration, User initializedBy)
    {
        var isInitialized = await IsInitialized();
        if (isInitialized)
            throw new InvalidOperationException("Mimisbrunner alredy initialized");
        
        await _repository.Create(configuration);
        await _userManager.ChangeRole(initializedBy, UserRole.Admin);
    }

    public Task<bool> IsInitialized()
    {
        return Task.FromResult(_repository.GetAll().Any());
    }

    public Task<ApplicationConfiguration> Get()
    {
        if(_memoryCache.TryGetValue(ConfigurationCacheKey, out ApplicationConfiguration configuration ))
            return Task.FromResult(configuration);
        
        configuration = _repository.GetAll().SingleOrDefault();
        if(configuration == null)
            return Task.FromResult<ApplicationConfiguration>(null);

        _memoryCache.Set(ConfigurationCacheKey, configuration, TimeSpan.FromMinutes(5));
        return Task.FromResult(configuration);
    }

    public async Task Configure(ApplicationConfiguration configuration)
    {
        var isInitialized = await IsInitialized();
        if (!isInitialized)
            throw new InvalidOperationException("Mimisbrunner not initialized");
        await _repository.Update(configuration);
        _memoryCache.Remove(ConfigurationCacheKey);
    }
}