using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Filters;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.DataImport;

namespace Mimisbrunnr.Web.Wiki;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[HandleWikiErrors]
public class SpaceController : ControllerBase
{
    private static readonly JsonSerializerOptions DeserializeSettings = new()
    {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
    };

    private readonly ISpaceService _spaceService;
    private readonly IDataImportService _spaceImportService;

    public SpaceController(ISpaceService spaceService, IDataImportService spaceImportService)
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
        return Ok(await _spaceService.GetAll(UserMapper.Instance.ToInfo(User)));
    }

    [HttpGet("{key}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SpaceModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Get([FromRoute] string key)
    {
        return Ok(await _spaceService.GetByKey(key, UserMapper.Instance.ToInfo(User)));
    }

    [HttpGet("{key}/permission")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserPermissionModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetPermission([FromRoute] string key)
    {
        return Ok(await _spaceService.GetPermission(key, UserMapper.Instance.ToInfo(User)));
    }

    [HttpGet("{key}/permissions")]
    [ProducesResponseType(typeof(SpacePermissionModel[]), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetPermissions([FromRoute] string key)
    {
        return Ok(await _spaceService.GetSpacePermissions(key, UserMapper.Instance.ToInfo(User)));
    }

    [HttpPost("{key}/permissions")]
    [ProducesResponseType(typeof(SpacePermissionModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AddPermissions([FromRoute] string key, [FromBody] SpacePermissionModel model)
    {
        return Ok(await _spaceService.AddPermission(key, model, UserMapper.Instance.ToInfo(User)));
    }

    [HttpPut("{key}/permissions")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdatePermissions([FromRoute] string key, [FromBody] SpacePermissionModel model)
    {
        await _spaceService.UpdatePermission(key, model, UserMapper.Instance.ToInfo(User));
        return Ok();
    }
    [HttpDelete("{key}/permissions")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeletePermissions([FromRoute] string key, [FromBody] SpacePermissionModel model)
    {
        await _spaceService.RemovePermission(key, model, UserMapper.Instance.ToInfo(User));
        return Ok();
    }

    [HttpPost]
    [ProducesResponseType(typeof(SpaceModel), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Create([FromBody] SpaceCreateModel createModel)
    {
        return Ok(await _spaceService.Create(createModel, UserMapper.Instance.ToInfo(User)));
    }

    [HttpPost("import")]
    [ProducesResponseType(typeof(SpaceModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [DisableRequestSizeLimit]
    [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
    public async Task<IActionResult> Import()
    {
        var createModelForm = HttpContext.Request.Form.FirstOrDefault(x => x.Key.Equals("model", StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrEmpty(createModelForm.Value.ToString()))
            return BadRequest();

        var createModel = JsonSerializer.Deserialize<SpaceCreateModel>(createModelForm.Value.ToString(), DeserializeSettings);
        var importZip = HttpContext.Request.Form.Files.FirstOrDefault(x => x.Name.Equals("import", StringComparison.OrdinalIgnoreCase));
        if (importZip == null)
            return BadRequest();

        var userInfo = UserMapper.Instance.ToInfo(User);

        var createdSpace = await _spaceService.Create(createModel, userInfo);
        await _spaceImportService.ImportSpace(createdSpace, importZip.OpenReadStream());

        return Ok(createdSpace);
    }

    [HttpPut("{key}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Update([FromRoute] string key, [FromBody] SpaceUpdateModel updateModel)
    {
        await _spaceService.Update(key, updateModel, UserMapper.Instance.ToInfo(User));
        return Ok();
    }

    [HttpPost("{key}/archive")]
    [ProducesResponseType(typeof(SpaceModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Archive([FromRoute] string key)
    {
        await _spaceService.Archive(key, UserMapper.Instance.ToInfo(User));
        return Ok();
    }

    [HttpPost("{key}/unarchive")]
    [ProducesResponseType(typeof(SpaceModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> UnArchive([FromRoute] string key)
    {
        await _spaceService.UnArchive(key, UserMapper.Instance.ToInfo(User));
        return Ok();
    }

    [HttpDelete("{key}")]
    [ProducesResponseType(typeof(SpaceModel), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Delete([FromRoute] string key)
    {
        await _spaceService.Remove(key, UserMapper.Instance.ToInfo(User));
        return Ok();
    }
}