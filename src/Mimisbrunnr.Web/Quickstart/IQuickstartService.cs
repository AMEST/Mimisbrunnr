using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Quickstart;

public interface IQuickstartService
{
    Task<bool> IsInitialized();
    
    Task Initialize(QuickstartModel model, UserInfo user);
}