using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Filters;
using Mimisbrunnr.Web.Mapping;

namespace Mimisbrunnr.Web.Wiki;

[Route("api/[controller]")]
[Authorize]
[HandleWikiErrors]
public class SpaceController : ControllerBase 
{
    private readonly ISpaceService _spaceService;

    public SpaceController(ISpaceService spaceService)
    {
        _spaceService = spaceService;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SpaceModel[]),200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _spaceService.GetAll(User?.ToEntity()));
    }

    [HttpGet("{key}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SpaceModel),200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Get([FromRoute] string key)
    {
        return Ok(await _spaceService.GetByKey(key, User?.ToEntity()));
    }
    
    [HttpGet("{key}/permission")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserPermissionModel),200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetPermission([FromRoute] string key)
    {
        return Ok(await _spaceService.GetPermission(key, User?.ToEntity()));
    }
    
    [HttpGet("{key}/permissions")]
    [ProducesResponseType(typeof(SpacePermissionModel[]),200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetPermissions([FromRoute] string key)
    {
        return Ok(await _spaceService.GetSpacePermissions(key, User?.ToEntity()));
    }
    
    [HttpPost("{key}/permissions")]
    [ProducesResponseType(typeof(SpacePermissionModel),200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AddPermissions([FromRoute] string key, [FromBody] SpacePermissionModel model)
    {
        return Ok(await _spaceService.AddPermission(key, model, User?.ToEntity()));
    }
    
    [HttpPut("{key}/permissions")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdatePermissions([FromRoute] string key, [FromBody] SpacePermissionModel model)
    {
        await _spaceService.UpdatePermission(key, model, User?.ToEntity());
        return Ok();
    }
    [HttpDelete("{key}/permissions")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleePermissions([FromRoute] string key, [FromBody] SpacePermissionModel model)
    {
        await _spaceService.RemovePermission(key, model, User?.ToEntity());
        return Ok();
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(SpaceModel),200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Create([FromBody] SpaceCreateModel createModel)
    {
        return Ok(await _spaceService.Create(createModel, User?.ToEntity()));
    }
    
    [HttpPut("{key}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Update([FromRoute] string key, [FromBody] SpaceUpdateModel updateModel)
    {
        await _spaceService.Update(key, updateModel, User?.ToEntity());
        return Ok();
    }
    
    [HttpPost("{key}/archive")]
    [ProducesResponseType(typeof(SpaceModel),200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Archive([FromRoute] string key)
    {
        await _spaceService.Archive(key, User?.ToEntity());
        return Ok();
    }
    
    [HttpPost("{key}/unarchive")]
    [ProducesResponseType(typeof(SpaceModel),200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> UnArchive([FromRoute] string key)
    {
        await _spaceService.UnArchive(key, User?.ToEntity());
        return Ok();
    }
    
    [HttpDelete("{key}")]
    [ProducesResponseType(typeof(SpaceModel),200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Delete([FromRoute] string key)
    {
        await _spaceService.Remove(key, User?.ToEntity());
        return Ok();
    }
}