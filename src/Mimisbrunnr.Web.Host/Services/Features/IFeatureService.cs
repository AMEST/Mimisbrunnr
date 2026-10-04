namespace Mimisbrunnr.Web.Host.Services.Features;

/// <summary>
/// Provides functionality for reading and updating feature flags
/// </summary>
public interface IFeatureService
{
    /// <summary>
    /// Determines whether the feature with the specified name is enabled
    /// </summary>
    /// <param name="name">The name of the feature to check</param>
    /// <returns>True if the feature is enabled; otherwise, false</returns>
    Task<bool> IsFeatureEnabled(string name);

    /// <summary>
    /// Sets the enabled state of the feature with the specified name
    /// </summary>
    /// <param name="name">The name of the feature to update</param>
    /// <param name="state">The state to set for the feature</param>
    Task SetFeatureState(string name, bool state);
}