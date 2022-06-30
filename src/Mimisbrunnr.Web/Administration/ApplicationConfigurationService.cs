using Microsoft.Extensions.Logging;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Web.Wiki;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Administration
{
    public class ApplicationConfigurationService : IApplicationConfigurationService
    {
        private readonly IApplicationConfigurationManager _configurationManager;
        private readonly ISpaceService _spaceService;
        private readonly ILogger<ApplicationConfigurationService> _logger;

        public ApplicationConfigurationService(
            IApplicationConfigurationManager configurationManager,
            ISpaceService spaceService,
            ILogger<ApplicationConfigurationService> logger
        )
        {
            _configurationManager = configurationManager;
            _spaceService = spaceService;
            _logger = logger;
        }

        public async Task<ApplicationConfigurationModel> Get()
        {
            var configuration = await _configurationManager.Get();
            return configuration.ToModel();
        }

        public async Task Update(ApplicationConfigurationModel model, UserInfo updatedBy)
        {
            var configuration = await _configurationManager.Get();
            configuration.Title = model.Title;
            configuration.AllowAnonymous = model.AllowAnonymous;
            configuration.AllowHtml = model.AllowHtml;
            configuration.SwaggerEnabled = model.SwaggerEnabled;
            configuration.CustomCss = model.CustomCss;
            configuration.CustomHomepageEnabled = model.CustomHomepageEnabled;
            configuration.CustomHomepageSpaceKey = model.CustomHomepageEnabled ? model.CustomHomepageSpaceKey : null;

            if(model.CustomHomepageEnabled && string.IsNullOrEmpty(model.CustomHomepageSpaceKey))
                throw new InvalidOperationException("Cannot enable custom homepage and not set space key whose home page is showed in home");
                
            if(model.CustomHomepageEnabled)
            {
                var space = await _spaceService.GetByKey(model.CustomHomepageSpaceKey, updatedBy);
                if(space.Type != SpaceTypeModel.Public)
                    throw new InvalidOperationException($"Cannot enable custom homepage and select space `{space.Key}` because is not public");
            }
            
            await _configurationManager.Configure(configuration);
            
            _logger.LogInformation("Application configuration updated by `{User}`", updatedBy.Email);
        }
    }
}