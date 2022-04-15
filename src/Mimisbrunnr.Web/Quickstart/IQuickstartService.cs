using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Quickstart;

public interface IQuickstartService
{
    Task<QuickstartModel> Get();
    
    Task<bool> IsInitialized();

    Task Initialize(QuickstartModel model, UserInfo user);
}