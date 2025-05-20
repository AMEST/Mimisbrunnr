using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Filters;
using Mimisbrunnr.Integration.Group;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Integration.User;

namespace Mimisbrunnr.Web.User;

/// <summary>
/// Controller for managing users
/// </summary>
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

    /// <summary>
    /// Get all users
    /// </summary>
    /// <param name="offset">Pagination offset</param>
    /// <returns>List of users</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll(int? offset = null)
    {
        return Ok(await _userService.GetUsers(User?.ToInfo(), offset));
    }

    /// <summary>
    /// Create a new user (admin only)
    /// </summary>
    /// <param name="model">User creation parameters</param>
    /// <returns>The created user</returns>
    [HttpPost]
    [RequiredAdminRole]
    public async Task<IActionResult> Create([FromBody] UserCreateModel model)
    {
        return Ok(await _userService.CreateUser(model, User?.ToInfo()));
    }

    /// <summary>
    /// Get the current authenticated user
    /// </summary>
    /// <returns>Current user information</returns>
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _userService.GetCurrent(User?.ToInfo());
        if (user == null)
            return NotFound();
        return Ok(user);
    }

    /// <summary>
    /// Get a user by email
    /// </summary>
    /// <param name="email">User's email</param>
    /// <returns>User information</returns>
    [HttpGet("{email}")]
    public async Task<IActionResult> Get([FromRoute] string email)
    {
        var user = await _userService.GetByEmail(email, User?.ToInfo());
        if (user == null)
            return NotFound();
        return Ok(user);
    }

    /// <summary>
    /// Get groups for a specific user
    /// </summary>
    /// <param name="email">User's email</param>
    /// <returns>List of groups the user belongs to</returns>
    [HttpGet("{email}/groups")]
    public async Task<IEnumerable<GroupModel>> GetUserGroups([FromRoute] string email)
    {
        var groups = await _userService.GetUserGroups(email, User?.ToInfo());
        return groups;
    }

    /// <summary>
    /// Update user profile information
    /// </summary>
    /// <param name="email">User's email</param>
    /// <param name="model">Profile update data</param>
    [HttpPut("{email}")]
    public async Task<IActionResult> Update([FromRoute] string email, [FromBody] UserProfileUpdateModel model)
    {
        await _userService.UpdateProfileInfo(email, model, User?.ToInfo());
        return Ok();
    }

    /// <summary>
    /// Disable a user account (admin only)
    /// </summary>
    /// <param name="email">User's email</param>
    [HttpPost("{email}/disable")]
    [RequiredAdminRole]
    public async Task<IActionResult> Disable([FromRoute] string email)
    {
        await _userService.Disable(email, User?.ToInfo());
        return Ok();
    }

    /// <summary>
    /// Enable a user account (admin only)
    /// </summary>
    /// <param name="email">User's email</param>
    [HttpPost("{email}/enable")]
    [RequiredAdminRole]
    public async Task<IActionResult> Enable([FromRoute] string email)
    {
        await _userService.Enable(email, User?.ToInfo());
        return Ok();
    }

    /// <summary>
    /// Promote a user to admin (admin only)
    /// </summary>
    /// <param name="email">User's email</param>
    [HttpPost("{email}/promote")]
    [RequiredAdminRole]
    public async Task<IActionResult> Promote([FromRoute] string email)
    {
        await _userService.Promote(email, User?.ToInfo());
        return Ok();
    }

    /// <summary>
    /// Demote a user from admin role (admin only)
    /// </summary>
    /// <param name="email">User's email</param>
    [HttpPost("{email}/demote")]
    [RequiredAdminRole]
    public async Task<IActionResult> Demote([FromRoute] string email)
    {
        await _userService.Demote(email, User?.ToInfo());
        return Ok();
    }
}
