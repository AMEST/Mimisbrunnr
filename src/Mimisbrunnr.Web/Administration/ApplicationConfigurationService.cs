using Microsoft.Extensions.Logging;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Administration
{
    public class ApplicationConfigurationService : IApplicationConfigurationService
    {
        private readonly IApplicationConfigurationManager _configurationManager;
        private readonly ILogger<ApplicationConfigurationService> _logger;

        public ApplicationConfigurationService(IApplicationConfigurationManager configurationManager, ILogger<ApplicationConfigurationService> logger)
        {
            _configurationManager = configurationManager;
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
            await _configurationManager.Configure(configuration);
            
            _logger.LogInformation("Application configuration updated by `{User}`", updatedBy.Email);
        }
    }
}