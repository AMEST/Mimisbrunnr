using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Mapping;

namespace Mimisbrunnr.Web.Quickstart;

/// <summary>
/// Controller for managing quickstart and initialization
/// </summary>
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

    /// <summary>
    /// Get the current quickstart state
    /// </summary>
    /// <returns>Quickstart configuration</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(QuickstartModel), 200)]
    public async Task<IActionResult> Get()
    {
        var state = await _quickstartService.Get();
        return Ok(state);
    }

    /// <summary>
    /// Get initialization status
    /// </summary>
    /// <returns>Whether the application is initialized</returns>
    [HttpGet("initialize")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(InitializeState), 200)]
    public async Task<IActionResult> GetInitializeStatus()
    {
        var state = await _quickstartService.IsInitialized();
        return Ok(new InitializeState() { IsInitialized = state });
    }

    /// <summary>
    /// Initialize the application
    /// </summary>
    /// <param name="model">Initialization parameters</param>
    /// <returns>200 if successful, 400 if initialization fails</returns>
    [HttpPost("initialize")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Initialize([FromBody] QuickstartModel model)
    {
        try
        {
            await _quickstartService.Initialize(model, User?.ToInfo());
            return Ok();
        }
        catch (InitializeException e)
        {
            return new ObjectResult(new { message = e.Message }) { StatusCode = 400 };
        }
    }
}
