using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Customization
{
    public interface ICustomizationService
    {
         Task<string> GetCustomCss();

         Task<CustomHomepageModel> GetCustomHomepage(UserInfo requestedBy);
    }
}