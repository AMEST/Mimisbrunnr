using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Mapping;

namespace Mimisbrunnr.Web.User;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _userService.GetCurrent(User.ToEntity());
        if (user == null)
            return NotFound();
        return Ok(user);
    }

    [HttpGet("{email}")]
    public async Task<IActionResult> Get([FromRoute] string email)
    {
        var user = await _userService.GetByEmail(email, User.ToEntity());
        if (user == null)
            return NotFound();
        return Ok(user);
    }
}