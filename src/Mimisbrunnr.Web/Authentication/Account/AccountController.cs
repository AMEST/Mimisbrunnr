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
    private readonly ITokenService _tokenService;

    public AccountController(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    [HttpGet("login")]
    [AllowAnonymous]
    public IActionResult Login([FromQuery] string redirectUri = null)
    {
        var redirect = "/";
        if(!string.IsNullOrEmpty(redirectUri) && redirectUri.StartsWith("/"))
            redirect = redirectUri;

        var user = User.ToEntity();
        if (user != null)
            return Redirect(redirect);

        var properties = new AuthenticationProperties()
        {
            RedirectUri = redirect
        };
        return Challenge(properties, "OpenIdConnect");
    }
    
    [HttpGet("[action]")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return Redirect("/");
    }

    [HttpGet("token")]
    [ProducesResponseType(typeof(IEnumerable<TokenModel>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetTokens()
    {
        var tokens = await _tokenService.GetUserTokens(User.ToEntity());
        return Ok(tokens);
    }

    [HttpPost("token")]
    [ProducesResponseType(typeof(TokenCreateResult), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CreateToken([FromBody] TokenCreateRequest request)
    {
        var token = await _tokenService.CreateUserToken(request, User.ToEntity());
        if(token is null)
            return BadRequest();

        return Ok(token);
    }

    [HttpDelete("token/{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CreateToken([FromRoute] string id)
    {
        await _tokenService.Revoke(id, User.ToEntity());
        return Ok();
    }
}