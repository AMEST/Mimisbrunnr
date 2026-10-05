using Microsoft.Extensions.Logging;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Web.Wiki;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Administration
{
    /// <summary>
    /// Service for reading and updating the application configuration
    /// </summary>
    public class ApplicationConfigurationService : IApplicationConfigurationService
    {
        private readonly IApplicationConfigurationManager _configurationManager;
        private readonly ISpaceService _spaceService;
        private readonly ILogger<ApplicationConfigurationService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationConfigurationService"/> class
        /// </summary>
        /// <param name="configurationManager">Manager used to read and persist the configuration</param>
        /// <param name="spaceService">Service used to validate the custom homepage space</param>
        /// <param name="logger">Logger instance</param>
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

        /// <summary>
        /// Gets the current application configuration
        /// </summary>
        /// <returns>The current application configuration model</returns>
        public async Task<ApplicationConfigurationModel> Get()
        {
            var configuration = await _configurationManager.Get();
            return configuration.ToModel();
        }

        /// <summary>
        /// Updates the application configuration
        /// </summary>
        /// <param name="model">The new configuration values</param>
        /// <param name="updatedBy">User performing the update</param>
        public async Task Update(ApplicationConfigurationModel model, UserInfo updatedBy)
        {
            var configuration = await _configurationManager.Get();
            configuration.Title = model.Title;
            configuration.AllowAnonymous = model.AllowAnonymous;
            configuration.UserAutoCreation = model.UserAutoCreation;
            configuration.AllowHtml = model.AllowHtml;
            configuration.SwaggerEnabled = model.SwaggerEnabled;
            configuration.McpEnabled = model.McpEnabled;
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