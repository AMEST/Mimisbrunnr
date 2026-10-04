using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Administration
{
    /// <summary>
    /// Service for reading and updating the application configuration
    /// </summary>
    public interface IApplicationConfigurationService
    {
         /// <summary>
         /// Gets the current application configuration
         /// </summary>
         /// <returns>The current application configuration model</returns>
         Task<ApplicationConfigurationModel> Get();

         /// <summary>
         /// Updates the application configuration
         /// </summary>
         /// <param name="model">The new configuration values</param>
         /// <param name="updatedBy">User performing the update</param>
         Task Update(ApplicationConfigurationModel model, UserInfo updatedBy);
    }
}