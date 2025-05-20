using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Mapping;

namespace Mimisbrunnr.Web.Authentication.Account;

/// <summary>
/// Controller for managing user authentication and tokens
/// </summary>
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

    /// <summary>
    /// Initiate login process with optional redirect
    /// </summary>
    /// <param name="redirectUri">URI to redirect to after login</param>
    /// <returns>Challenge result for authentication</returns>
    [HttpGet("login")]
    [AllowAnonymous]
    public IActionResult Login([FromQuery] string redirectUri = null)
    {
        var redirect = "/";
        if(!string.IsNullOrEmpty(redirectUri) && redirectUri.StartsWith("/"))
            redirect = redirectUri;

        var user = User?.ToInfo();
        if (user != null)
            return Redirect(redirect);

        var properties = new AuthenticationProperties()
        {
            RedirectUri = redirect
        };
        return Challenge(properties, "OpenIdConnect");
    }
    
    /// <summary>
    /// Log out the current user
    /// </summary>
    /// <returns>Redirect to home page</returns>
    [HttpGet("[action]")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return Redirect("/");
    }

    /// <summary>
    /// Get all tokens for the current user
    /// </summary>
    /// <returns>List of user tokens</returns>
    [HttpGet("token")]
    [ProducesResponseType(typeof(IEnumerable<TokenModel>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetTokens()
    {
        var tokens = await _tokenService.GetUserTokens(User?.ToInfo());
        return Ok(tokens);
    }

    /// <summary>
    /// Create a new token for the current user
    /// </summary>
    /// <param name="request">Token creation parameters</param>
    /// <returns>Created token information</returns>
    [HttpPost("token")]
    [ProducesResponseType(typeof(TokenCreateResult), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CreateToken([FromBody] TokenCreateRequest request)
    {
        var token = await _tokenService.CreateUserToken(request, User?.ToInfo());
        if(token is null)
            return BadRequest();

        return Ok(token);
    }

    /// <summary>
    /// Revoke/delete a token
    /// </summary>
    /// <param name="id">ID of the token to revoke</param>
    [HttpDelete("token/{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CreateToken([FromRoute] string id)
    {
        await _tokenService.Revoke(id, User?.ToInfo());
        return Ok();
    }
}
