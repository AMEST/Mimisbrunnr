using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Filters;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Integration.Plugin;

namespace Mimisbrunnr.Web.Plugin;

/// <summary>
/// API controller for managing plugins and macros
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
[HandlePluginErrors]
[HandleWikiErrors]
public class PluginController : ControllerBase
{
    private readonly IPluginService _pluginService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginController"/> class
    /// </summary>
    /// <param name="pluginService">Service providing plugin and macro operations</param>
    public PluginController(IPluginService pluginService)
    {
        _pluginService = pluginService;
    }

    /// <summary>
    /// Retrieves available macros with pagination support
    /// </summary>
    /// <param name="skip">Number of items to skip</param>
    /// <param name="take">Number of items to take</param>
    /// <returns>Array of available macro models</returns>
    [HttpGet("macros")]
    public Task<MacroModel[]> GetAvailableMacroses([FromQuery] int? skip, [FromQuery] int? take)
    {
        return _pluginService.GetAvailableMacroses(skip, take);
    }

    /// <summary>
    /// Retrieves information about a specific macro
    /// </summary>
    /// <param name="macroIdentifier">Unique identifier of the macro</param>
    /// <returns>Macro model containing macro information</returns>
    [HttpGet("macros/{macroIdentifier}")]
    public Task<MacroModel> GetMacroInfo([FromRoute] string macroIdentifier)
    {
        return _pluginService.GetMacroInfo(macroIdentifier);
    }

    /// <summary>
    /// Retrieves the state of a macro on a specific page
    /// </summary>
    /// <param name="pageId">Unique identifier of the page</param>
    /// <param name="macroIdOnPage">Unique identifier of the macro on the page</param>
    /// <returns>Current state of the macro on the specified page</returns>
    [HttpGet("macros/{pageId}/{macroIdOnPage}/state")]
    [AllowAnonymous]
    public Task<MacroStateModel> GetMacroState([FromRoute] string pageId, [FromRoute] string macroIdOnPage)
    {
        var userInfo = User?.ToInfo();
        return _pluginService.GetMacroState(pageId, macroIdOnPage, userInfo);
    }

    /// <summary>
    /// Renders a macro on a specific page
    /// </summary>
    /// <param name="pageId">Unique identifier of the page containing the macro</param>
    /// <param name="macroIdOnPage">Unique identifier of the macro on the page</param>
    /// <param name="userRequest">Request containing user-specific rendering parameters</param>
    /// <returns>Response containing the rendered macro content</returns>
    [HttpPost("macros/{pageId}/{macroIdOnPage}/render")]
    [AllowAnonymous]
    public Task<MacroRenderResponse> Render([FromRoute] string pageId, [FromRoute] string macroIdOnPage, [FromBody] MacroRenderUserRequest userRequest)
    {
        var userInfo = User?.ToInfo();
        return _pluginService.Render(pageId, macroIdOnPage, userRequest, userInfo);
    }

    /// <summary>
    /// Saves the state of a macro
    /// </summary>
    /// <param name="state">State model containing the new state</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    [HttpPost("macros/state")]
    public Task SaveMacroState([FromBody] MacroStateModel state)
    {
        var userInfo = User?.ToInfo();
        return _pluginService.SaveMacroState(state, userInfo);
    }

    /// <summary>
    /// Retrieves installed plugins with pagination support
    /// </summary>
    /// <param name="skip">Number of items to skip</param>
    /// <param name="take">Number of items to take</param>
    /// <returns>Array of installed plugin models</returns>
    [HttpGet]
    [RequiredAdminRole]
    public Task<PluginModel[]> GetPlugins([FromQuery] int? skip, [FromQuery] int? take)
    {
        return _pluginService.GetPlugins(skip, take);
    }

    /// <summary>
    /// Installs a new plugin in the system
    /// </summary>
    /// <param name="model">The plugin model containing plugin details</param>
    /// <returns>The installed plugin model</returns>
    [HttpPost]
    [RequiredAdminRole]
    public Task<PluginModel> InstallPlugin([FromBody] PluginModel model)
    {
        var userInfo = User?.ToInfo();
        return _pluginService.InstallPlugin(model, userInfo);
    }

    /// <summary>
    /// Uninstalls a plugin from the system
    /// </summary>
    /// <param name="macroIdentifier">Unique identifier of the plugin to uninstall</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    [HttpDelete("{macroIdentifier}")]
    [RequiredAdminRole]
    public Task UnInstallPlugin([FromRoute] string macroIdentifier)
    {
        var userInfo = User?.ToInfo();
        return _pluginService.UnInstallPlugin(macroIdentifier, userInfo);
    }

    /// <summary>
    /// Disables an enabled plugin
    /// </summary>
    /// <param name="pluginIdentifier">Unique identifier of the plugin to disable</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    [HttpPost("{pluginIdentifier}/disable")]
    [RequiredAdminRole]
    public Task DisablePlugin([FromRoute] string pluginIdentifier)
    {
        var userInfo = User?.ToInfo();
        return _pluginService.DisablePlugin(pluginIdentifier, userInfo);
    }

    /// <summary>
    /// Enables a previously disabled plugin
    /// </summary>
    /// <param name="pluginIdentifier">Unique identifier of the plugin to enable</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    [HttpPost("{pluginIdentifier}/enable")]
    [RequiredAdminRole]
    public Task EnablePlugin([FromRoute] string pluginIdentifier)
    {
        var userInfo = User?.ToInfo();
        return _pluginService.EnablePlugin(pluginIdentifier, userInfo);
    }
}
