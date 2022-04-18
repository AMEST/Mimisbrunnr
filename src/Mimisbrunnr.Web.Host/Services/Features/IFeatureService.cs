namespace Mimisbrunnr.Web.Host.Services.Features;

public interface IFeatureService
{
    Task<bool> IsFeatureEnabled(string name);

    Task SetFeatureState(string name, bool state);
}