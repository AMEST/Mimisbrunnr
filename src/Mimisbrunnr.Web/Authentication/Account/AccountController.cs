using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Mapping;

namespace Mimisbrunnr.Web.Authentication.Account;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AccountController : ControllerBase
{
    [HttpGet("current")]
    public IActionResult GetCurrentUser()
    {
        var user = User.ToEntity();
        if (user == null)
            return NotFound();
        return Ok(user.ToModel());
    }

    [HttpGet("login")]
    [AllowAnonymous]
    public IActionResult Login()
    {
        var user = User.ToEntity();
        if (user != null)
            return Redirect("/");

        var properties = new AuthenticationProperties()
        {
            RedirectUri = "/"
        };
        return Challenge(properties, "OpenIdConnect");
    }
    
    [HttpGet("[action]")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return Redirect("/");
    }
}