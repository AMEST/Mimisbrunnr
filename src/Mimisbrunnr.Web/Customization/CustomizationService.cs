using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Customization
{
    internal class CustomizationService : ICustomizationService
    {
        private readonly IPermissionService _permissionService;
        private readonly IApplicationConfigurationManager _configurationManager;

        public CustomizationService(
            IPermissionService permissionService,
            IApplicationConfigurationManager configurationManager
        )
        {
            _permissionService = permissionService;
            _configurationManager = configurationManager;
        }

        public async Task<string> GetCustomCss()
        {
            var configuration = await _configurationManager.Get();
            if (configuration is null)
                return string.Empty;
            return configuration.CustomCss;
        }

        public async Task<CustomHomepageModel> GetCustomHomepage(UserInfo requestedBy)
        {
            await _permissionService.EnsureAnonymousAllowed(requestedBy);
            var configuration = await _configurationManager.Get();
            if (!configuration.CustomHomepageEnabled)
                return null;

            return new CustomHomepageModel(){
                HomepageSpaceKey = configuration.CustomHomepageSpaceKey
            };
        }
    }
}