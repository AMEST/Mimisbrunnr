using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Mapping;

namespace Mimisbrunnr.Web.Quickstart;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class QuickstartController : ControllerBase
{
    private readonly IQuickstartService _quickstartService;

    public QuickstartController(IQuickstartService quickstartService)
    {
        _quickstartService = quickstartService;
    }

    [HttpGet("initialize")]
    [ProducesResponseType(typeof(InitializeState), 200)]
    public async Task<IActionResult> GetInitializeStatus()
    { 
        var state = await _quickstartService.IsInitialized();
        return Ok(new InitializeState() { IsInitialized = state });
    }
    
    [HttpPost("initialize")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Initialize([FromBody] QuickstartModel model)
    {
        try
        {
            await _quickstartService.Initialize(model, User.ToEntity());
            return Ok();
        }
        catch (InitializeException e)
        {
            return new ObjectResult(new { message = e.Message }){StatusCode = 400};
        }
    }
}