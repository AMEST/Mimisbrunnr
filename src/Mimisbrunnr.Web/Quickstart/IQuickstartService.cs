using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Quickstart;

/// <summary>
/// Service for managing application quickstart and initialization
/// </summary>
public interface IQuickstartService
{
    /// <summary>
    /// Gets the current quickstart configuration
    /// </summary>
    /// <returns>The quickstart configuration model</returns>
    Task<QuickstartModel> Get();
    
    /// <summary>
    /// Determines whether the application has been initialized
    /// </summary>
    /// <returns>True if the application is initialized; otherwise false</returns>
    Task<bool> IsInitialized();

    /// <summary>
    /// Initializes the application with the specified parameters
    /// </summary>
    /// <param name="model">Initialization parameters</param>
    /// <param name="user">User performing the initialization</param>
    Task Initialize(QuickstartModel model, UserInfo user);
}