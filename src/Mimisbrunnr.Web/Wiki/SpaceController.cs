using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Mimisbrunnr.Web.Filters;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Web.Wiki.Import;

namespace Mimisbrunnr.Web.Wiki;

[Route("api/[controller]")]
[Authorize]
[HandleWikiErrors]
public class SpaceController : ControllerBase
{
    private readonly ISpaceService _spaceService;
    private readonly ISpaceImportService _spaceImportService;

    public SpaceController(ISpaceService spaceService, ISpaceImportService spaceImportService)
    {
        _spaceImportService = spaceImportService;
        _spaceService = spaceService;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SpaceModel[]), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _spaceService.GetAll(User?.ToEntity()));
    }

    [HttpGet("{key}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SpaceModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Get([FromRoute] string key)
    {
        return Ok(await _spaceService.GetByKey(key, User?.ToEntity()));
    }

    [HttpGet("{key}/permission")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserPermissionModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetPermission([FromRoute] string key)
    {
        return Ok(await _spaceService.GetPermission(key, User?.ToEntity()));
    }

    [HttpGet("{key}/permissions")]
    [ProducesResponseType(typeof(SpacePermissionModel[]), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetPermissions([FromRoute] string key)
    {
        return Ok(await _spaceService.GetSpacePermissions(key, User?.ToEntity()));
    }

    [HttpPost("{key}/permissions")]
    [ProducesResponseType(typeof(SpacePermissionModel), 200)]
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
    public async Task<IActionResult> DeletePermissions([FromRoute] string key, [FromBody] SpacePermissionModel model)
    {
        await _spaceService.RemovePermission(key, model, User?.ToEntity());
        return Ok();
    }

    [HttpPost]
    [ProducesResponseType(typeof(SpaceModel), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Create([FromBody] SpaceCreateModel createModel)
    {
        return Ok(await _spaceService.Create(createModel, User?.ToEntity()));
    }

    [HttpPost("import")]
    [ProducesResponseType(typeof(SpaceModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Import()
    {
        var createModelForm = HttpContext.Request.Form.FirstOrDefault(x => x.Key.Equals("model", StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrEmpty(createModelForm.Value.ToString()))
            return BadRequest();

        var deserializeSettings = new JsonSerializerOptions{
            PropertyNameCaseInsensitive = true,
        };
        deserializeSettings.Converters.Add(new JsonStringEnumConverter());

        var createModel = JsonSerializer.Deserialize<SpaceCreateModel>(createModelForm.Value.ToString(), deserializeSettings);
        var importZip = HttpContext.Request.Form.Files.FirstOrDefault(x => x.Name.Equals("import", StringComparison.OrdinalIgnoreCase));
        using var importZipStream = new MemoryStream();
        await importZip.CopyToAsync(importZipStream);

        var userInfo = User?.ToEntity();

        var createdSpace = await _spaceService.Create(createModel, userInfo);
        await _spaceImportService.Import(createdSpace, importZipStream, userInfo);

        return Ok(createdSpace);
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
    [ProducesResponseType(typeof(SpaceModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Archive([FromRoute] string key)
    {
        await _spaceService.Archive(key, User?.ToEntity());
        return Ok();
    }

    [HttpPost("{key}/unarchive")]
    [ProducesResponseType(typeof(SpaceModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> UnArchive([FromRoute] string key)
    {
        await _spaceService.UnArchive(key, User?.ToEntity());
        return Ok();
    }

    [HttpDelete("{key}")]
    [ProducesResponseType(typeof(SpaceModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Delete([FromRoute] string key)
    {
        await _spaceService.Remove(key, User?.ToEntity());
        return Ok();
    }
}