using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Filters;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Integration.Plugin;

namespace Mimisbrunnr.Web.Plugin;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PluginController : ControllerBase
{
    private readonly IPluginService _pluginService;

    public PluginController(IPluginService pluginService)
    {
        _pluginService = pluginService;
    }

    [HttpGet("macros")]
    public Task<MacroModel[]> GetAvailableMacroses([FromQuery] int? skip, [FromQuery] int? take)
    {
        return _pluginService.GetAvailableMacroses(skip, take);
    }

    [HttpGet("macros/{macroIdentifier}")]
    public Task<MacroModel> GetMacroInfo([FromRoute] string macroIdentifier)
    {
        return _pluginService.GetMacroInfo(macroIdentifier);
    }

    [HttpGet("macros/{pageId}/{macroIdOnPage}/state")]
    public Task<MacroStateModel> GetMacroState([FromRoute] string pageId, [FromRoute] string macroIdOnPage)
    {
        var userInfo = User?.ToInfo();
        return _pluginService.GetMacroState(pageId, macroIdOnPage, userInfo);
    }

    [HttpPost("macros/{pageId}/{macroIdOnPage}/render")]
    public Task<MacroStateModel> Render([FromRoute] string pageId, [FromRoute] string macroIdOnPage, [FromBody] MacroRenderUserRequest userRequest)
    {
        var userInfo = User?.ToInfo();
        return _pluginService.GetMacroState(pageId, macroIdOnPage, userInfo);
    }

    [HttpPost("macros/state")]
    public Task SaveMacroState([FromBody] MacroStateModel state)
    {
        var userInfo = User?.ToInfo();
        return _pluginService.SaveMacroState(state, userInfo);
    }

    [HttpGet]
    [RequiredAdminRole]
    public Task<PluginModel[]> GetPlugins([FromQuery] int? skip, [FromQuery] int? take)
    {
        return _pluginService.GetPlugins(skip, take);
    }

    [HttpPost]
    [RequiredAdminRole]
    public Task<PluginModel> InstallPlugin([FromBody] PluginModel model)
    {
        var userInfo = User?.ToInfo();
        return _pluginService.InstallPlugin(model, userInfo);
    }

    [HttpDelete("{macroIdentifier}")]
    [RequiredAdminRole]
    public Task UnInstallPlugin([FromRoute] string macroIdentifier)
    {
        var userInfo = User?.ToInfo();
        return _pluginService.UnInstallPlugin(macroIdentifier, userInfo);
    }

    [HttpPost("{macroIdentifier}/disable")]
    [RequiredAdminRole]
    public Task DisablePlugin([FromRoute] string macroIdentifier)
    {
        var userInfo = User?.ToInfo();
        return _pluginService.DisablePlugin(macroIdentifier, userInfo);
    }

    [HttpPost("{macroIdentifier}/enable")]
    [RequiredAdminRole]
    public Task EnablePlugin([FromRoute] string macroIdentifier)
    {
        var userInfo = User?.ToInfo();
        return _pluginService.EnablePlugin(macroIdentifier, userInfo);
    }
}
