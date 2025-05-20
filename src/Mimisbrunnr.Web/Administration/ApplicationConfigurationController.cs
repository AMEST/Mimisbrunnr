using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Filters;
using Mimisbrunnr.Web.Mapping;

namespace Mimisbrunnr.Web.Administration;

/// <summary>
/// Controller for managing application configuration (admin only)
/// </summary>
[Route("api/admin/[controller]")]
[ApiController]
[Authorize]
[RequiredAdminRole]
public class ApplicationConfigurationController : ControllerBase
{
    private readonly IApplicationConfigurationService _configurationService;

    public ApplicationConfigurationController(IApplicationConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }

    /// <summary>
    /// Get the current application configuration
    /// </summary>
    /// <returns>The application configuration</returns>
    [HttpGet]
    public Task<ApplicationConfigurationModel> Get() => _configurationService.Get();

    /// <summary>
    /// Update the application configuration
    /// </summary>
    /// <param name="model">The new configuration values</param>
    [HttpPut]
    public Task Update([FromBody] ApplicationConfigurationModel model) => _configurationService.Update(model, User?.ToInfo());
}
