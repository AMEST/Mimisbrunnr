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

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(QuickstartModel), 200)]
    public async Task<IActionResult> Get()
    {
        var state = await _quickstartService.Get();
        return Ok(state);
    }

    [HttpGet("initialize")]
    [AllowAnonymous]
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
            await _quickstartService.Initialize(model, UserMapper.Instance.ToInfo(User));
            return Ok();
        }
        catch (InitializeException e)
        {
            return new ObjectResult(new { message = e.Message }) { StatusCode = 400 };
        }
    }
}