using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Filters;
using Mimisbrunnr.Integration.Group;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Integration.User;

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
    public async Task<IActionResult> GetAll(int? offset = null)
    {
        return Ok(await _userService.GetUsers(User?.ToInfo(), offset));
    }

    [HttpPost]
    [RequiredAdminRole]
    public async Task<IActionResult> Create([FromBody] UserCreateModel model)
    {
        return Ok(await _userService.CreateUser(model, User?.ToInfo()));
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _userService.GetCurrent(User?.ToInfo());
        if (user == null)
            return NotFound();
        return Ok(user);
    }


    [HttpGet("{email}")]
    public async Task<IActionResult> Get([FromRoute] string email)
    {
        var user = await _userService.GetByEmail(email, User?.ToInfo());
        if (user == null)
            return NotFound();
        return Ok(user);
    }

    [HttpGet("{email}/groups")]
    public async Task<IEnumerable<GroupModel>> GetUserGroups([FromRoute] string email)
    {
        var groups = await _userService.GetUserGroups(email, User?.ToInfo());
        return groups;
    }

    [HttpPut("{email}")]
    public async Task<IActionResult> Update([FromRoute] string email, [FromBody] UserProfileUpdateModel model)
    {
        await _userService.UpdateProfileInfo(email, model, User?.ToInfo());
        return Ok();
    }

    [HttpPost("{email}/disable")]
    [RequiredAdminRole]
    public async Task<IActionResult> Disable([FromRoute] string email)
    {
        await _userService.Disable(email, User?.ToInfo());
        return Ok();
    }

    [HttpPost("{email}/enable")]
    [RequiredAdminRole]
    public async Task<IActionResult> Enable([FromRoute] string email)
    {
        await _userService.Enable(email, User?.ToInfo());
        return Ok();
    }

    [HttpPost("{email}/promote")]
    [RequiredAdminRole]
    public async Task<IActionResult> Promote([FromRoute] string email)
    {
        await _userService.Promote(email, User?.ToInfo());
        return Ok();
    }

    [HttpPost("{email}/demote")]
    [RequiredAdminRole]
    public async Task<IActionResult> Demote([FromRoute] string email)
    {
        await _userService.Demote(email, User?.ToInfo());
        return Ok();
    }
}