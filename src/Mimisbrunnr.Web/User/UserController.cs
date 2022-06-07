using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Filters;
using Mimisbrunnr.Web.Group;
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

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok( await _userService.GetUsers(User.ToEntity()));
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

    [HttpGet("{email}/groups")]
    public async Task<IEnumerable<GroupModel>> GetUserGroups([FromRoute] string email)
    {
        var groups = await _userService.GetUserGroups(email, User.ToEntity());
        return groups;
    }

    [HttpPost("{email}/disable")]
    [RequiredAdminRole]
    public async Task<IActionResult> Disable([FromRoute] string email)
    {
        await _userService.Disable(email, User.ToEntity());
        return Ok();
    }

    [HttpPost("{email}/enable")]
    [RequiredAdminRole]
    public async Task<IActionResult> Enable([FromRoute] string email)
    {
        await _userService.Enable(email, User.ToEntity());
        return Ok();
    }

    [HttpPost("{email}/promote")]
    [RequiredAdminRole]
    public async Task<IActionResult> Promote([FromRoute] string email)
    {
        await _userService.Promote(email, User.ToEntity());
        return Ok();
    }

    [HttpPost("{email}/demote")]
    [RequiredAdminRole]
    public async Task<IActionResult> Demote([FromRoute] string email)
    {
        await _userService.Demote(email, User.ToEntity());
        return Ok();
    }
}