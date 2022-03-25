using Mimisbrunner.Users;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Quickstart;

internal class QuickstartService : IQuickstartService
{
    private readonly IApplicationConfigurationService _applicationConfigurationService;
    private readonly IUserManager _userManager;

    public QuickstartService(
        IApplicationConfigurationService applicationConfigurationService,
        IUserManager userManager
    )
    {
        _applicationConfigurationService = applicationConfigurationService;
        _userManager = userManager;
    }

    public async Task<QuickstartModel> Get()
    {
        return (await _applicationConfigurationService.Get()).ToModel();
    }

    public Task<bool> IsInitialized()
    {
        return _applicationConfigurationService.IsInitialized();
    }

    public async Task Initialize(QuickstartModel model, UserInfo user)
    {
        if (user == null)
            throw new InitializeException("Unknown user");
        
        var isInitialized = await _applicationConfigurationService.IsInitialized();
        if (isInitialized)
            throw new InitializeException("Application already initialized");
        
        var applicationUser = await _userManager.FindByEmail(user.Email);
        await _applicationConfigurationService.Initialize(model.ToEntity(), applicationUser);
    }
}