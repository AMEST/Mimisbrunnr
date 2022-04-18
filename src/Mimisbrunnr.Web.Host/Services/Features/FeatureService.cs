using Mimisbrunnr.Web.Infrastructure;

namespace Mimisbrunnr.Web.Host.Services.Features;

internal class FeatureService : IFeatureService
{
    public const string ApplicationFeaturePrefix = "appconfig_";

    private readonly IApplicationConfigurationService _applicationConfigurationService;

    public FeatureService(IApplicationConfigurationService applicationConfigurationService)
    {
        _applicationConfigurationService = applicationConfigurationService;
    }

    public Task<bool> IsFeatureEnabled(string name)
    {
        if (name.StartsWith(ApplicationFeaturePrefix))
            return ApplicationConfigurationFeatures(name);

        throw new NotImplementedException("Custom features not implemented");
    }

    public Task SetFeatureState(string name, bool state)
    {
        throw new NotImplementedException("Custom features not implemented");
    }

    private async Task<bool> ApplicationConfigurationFeatures(string name)
    {
        var configuration = await _applicationConfigurationService.Get();
        if(configuration is null)
            return false;
            
        switch (name)
        {
            case $"{ApplicationFeaturePrefix}_swagger":
                return configuration.SwaggerEnabled;
            default:
                throw new ArgumentOutOfRangeException($"Unknown application feature {name}");
        }
    }
}