namespace Mimisbrunnr.Web.Host.Services.Features;

internal interface IFeatureService
{
    Task<bool> IsFeatureEnabled(string name);

    Task SetFeatureState(string name, bool state);
}