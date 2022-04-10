using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Mapping;

namespace Mimisbrunnr.Web.User;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
    private IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("current")]
    [AllowAnonymous]
    public IActionResult GetCurrentUser()
    {
        var user = User.ToEntity();
        if (user == null)
            return NotFound();
        return Ok(user.ToModel());
    }

    [HttpGet("find")]
    public async Task<IActionResult> Find([FromQuery] string email)
    {
        var user = await _userService.FindByEmail(email, User.ToEntity());
        if (user == null)
            return NotFound();
        return Ok(user);
    }
}