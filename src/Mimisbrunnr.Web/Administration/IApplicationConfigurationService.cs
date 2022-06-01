using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Administration
{
    public interface IApplicationConfigurationService
    {
         Task<ApplicationConfigurationModel> Get();

         Task Update(ApplicationConfigurationModel model, UserInfo updatedBy);
    }
}