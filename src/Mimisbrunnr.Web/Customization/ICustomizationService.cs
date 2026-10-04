using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Customization
{
    /// <summary>
    /// Service for managing UI customization
    /// </summary>
    public interface ICustomizationService
    {
         /// <summary>
         /// Gets the custom CSS applied to the application
         /// </summary>
         /// <returns>The custom CSS content</returns>
         Task<string> GetCustomCss();

         /// <summary>
         /// Gets the custom homepage configuration for the specified user
         /// </summary>
         /// <param name="requestedBy">User requesting the custom homepage</param>
         /// <returns>The custom homepage model</returns>
         Task<CustomHomepageModel> GetCustomHomepage(UserInfo requestedBy);
    }
}