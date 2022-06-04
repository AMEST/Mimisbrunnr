using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Infrastructure.Contracts;

namespace Mimisbrunnr.Web.Infrastructure;

public interface IApplicationConfigurationManager
{
    Task Initialize(ApplicationConfiguration configuration, User administator);
    
    Task<bool> IsInitialized();

    Task<ApplicationConfiguration> Get();

    Task Configure(ApplicationConfiguration configuration);
}