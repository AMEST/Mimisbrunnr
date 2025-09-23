using Mimisbrunnr.Integration.Plugin;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Plugin;

/// <summary>
/// Provides functionality for managing plugins and macros
/// </summary>
public interface IPluginService
{
    /// <summary>
    /// Retrieves available macros with pagination support
    /// </summary>
    /// <param name="skip">Number of items to skip</param>
    /// <param name="take">Number of items to take</param>
    /// <returns>Array of available macro models</returns>
    Task<MacroModel[]> GetAvailableMacroses(int? skip = null, int? take = null);

    /// <summary>
    /// Retrieves information about a specific macro
    /// </summary>
    /// <param name="macroIdentifier">Unique identifier of the macro</param>
    /// <returns>Macro model containing macro information</returns>
    Task<MacroModel> GetMacroInfo(string macroIdentifier);

    /// <summary>
    /// Retrieves installed plugins with pagination support
    /// </summary>
    /// <param name="skip">Number of items to skip</param>
    /// <param name="take">Number of items to take</param>
    /// <returns>Array of installed plugin models</returns>
    Task<PluginModel[]> GetPlugins(int? skip = null, int? take = null);

    /// <summary>
    /// Installs a new plugin in the system
    /// </summary>
    /// <param name="model">The plugin model containing plugin details</param>
    /// <param name="userInfo">The user information of the user installing the plugin</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the installed plugin model</returns>
    Task<PluginModel> InstallPlugin(PluginModel model, UserInfo userInfo);

    /// <summary>
    /// Uninstalls a plugin from the system
    /// </summary>
    /// <param name="id">Unique identifier of the plugin to uninstall</param>
    /// <param name="userInfo">User information of the user performing the operation</param>
    Task UnInstallPlugin(string id, UserInfo userInfo);

    /// <summary>
    /// Enables a previously disabled plugin
    /// </summary>
    /// <param name="pluginIdentifier">Unique identifier of the plugin to enable</param>
    /// <param name="userInfo">User information of the user performing the operation</param>
    Task EnablePlugin(string pluginIdentifier, UserInfo userInfo);

    /// <summary>
    /// Disables an enabled plugin
    /// </summary>
    /// <param name="pluginIdentifier">Unique identifier of the plugin to disable</param>
    /// <param name="userInfo">User information of the user performing the operation</param>
    Task DisablePlugin(string pluginIdentifier, UserInfo userInfo);

    /// <summary>
    /// Retrieves the state of a macro on a specific page
    /// </summary>
    /// <param name="pageId">Unique identifier of the page</param>
    /// <param name="macroIdOnPage">Unique identifier of the macro on the page</param>
    /// <param name="userInfo">User information of the user requesting the state</param>
    /// <returns>Current state of the macro on the specified page</returns>
    Task<MacroStateModel> GetMacroState(string pageId, string macroIdOnPage, UserInfo userInfo);

    /// <summary>
    /// Saves the state of a macro
    /// </summary>
    /// <param name="state">State model containing the new state</param>
    /// <param name="userInfo">User information of the user saving the state</param>
    Task SaveMacroState(MacroStateModel state, UserInfo userInfo);

    /// <summary>
    /// Renders a macro on a specific page
    /// </summary>
    /// <param name="pageId">Unique identifier of the page containing the macro</param>
    /// <param name="macroIdOnPage">Unique identifier of the macro on the page</param>
    /// <param name="userRequest">Request containing user-specific rendering parameters</param>
    /// <param name="userInfo">User information of the user requesting the render</param>
    /// <returns>Response containing the rendered macro content</returns>
    Task<MacroRenderResponse> Render(string pageId, string macroIdOnPage, MacroRenderUserRequest userRequest, UserInfo userInfo);
}
