using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Filters;
using Mimisbrunnr.Web.Mapping;

namespace Mimisbrunnr.Web.Administration;

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

    [HttpGet]
    public Task<ApplicationConfigurationModel> Get() => _configurationService.Get();

    [HttpPut]
    public Task Update([FromBody] ApplicationConfigurationModel model) => _configurationService.Update(model, User?.ToInfo());
}