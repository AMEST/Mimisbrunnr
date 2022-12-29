using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Filters;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Integration.User;
using Mimisbrunnr.Integration.Group;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Group;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[HandleGroupErrors]
public class GroupController : ControllerBase
{
    private readonly IGroupService _groupService;

    public GroupController(IGroupService groupService)
    {
        _groupService = groupService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(GroupModel[]), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetAll([FromQuery] GroupFilterModel filter = null )
    {
        return Ok(await _groupService.GetAll(filter, User?.ToInfo()));
    }

    [HttpGet("{name}")]
    [ProducesResponseType(typeof(GroupModel), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Get([FromRoute] string name)
    {
        return Ok(await _groupService.Get(name, User?.ToInfo()));
    }

    [HttpGet("{name}/users")]
    [ProducesResponseType(typeof(UserModel[]), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetUsers([FromRoute] string name)
    {
        return Ok(await _groupService.GetUsers(name, User?.ToInfo()));
    }


    [HttpPost]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Create([FromBody] GroupCreateModel model)
    {
        await _groupService.Create(model, User?.ToInfo());
        return Ok();
    }

    [HttpPut("{name}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update([FromRoute] string name, [FromBody] GroupUpdateModel model)
    {
        await _groupService.Update(name, model, User?.ToInfo());
        return Ok();
    }

    [HttpDelete("{name}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Remove([FromRoute] string name)
    {
        await _groupService.Remove(name, User?.ToInfo());
        return Ok();
    }

    [HttpPost("{name}/{email}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AddToGroup([FromRoute] string name, [FromRoute] string email)
    {
        await _groupService.AddUserToGroup(name, new UserInfo(){Email = email}, User?.ToInfo());
        return Ok();
    }

    [HttpDelete("{name}/{email}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RemoveFromGroup([FromRoute] string name, [FromRoute] string email)
    {
        await _groupService.RemoveUserFromGroup(name, new UserInfo(){Email = email}, User?.ToInfo());
        return Ok();
    }
}