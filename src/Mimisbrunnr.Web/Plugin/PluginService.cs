using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Mimisbrunnr.Integration.Plugin;
using Mimisbrunnr.Integration.User;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Web.Plugin;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Services;

/// <summary>
/// Provides functionality for managing plugins and macros
/// </summary>
public class PluginService : IPluginService
{
    private readonly IPermissionService _permissionService;
    private readonly IPluginManager _pluginManager;
    private readonly IPageManager _pageManager;
    private readonly ISpaceManager _spaceManager;
    private readonly IUserManager _userManager;
    private readonly ITemplateRenderer _templateRenderer;
    private readonly ISecurityTokenService _tokenService;
    private readonly ILogger<PluginService> _logger;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginService"/> class
    /// </summary>
    /// <param name="permissionService">Service used to check user permissions</param>
    /// <param name="pluginManager">Manager providing plugin and macro operations</param>
    /// <param name="pageManager">Manager providing page operations</param>
    /// <param name="spaceManager">Manager providing space operations</param>
    /// <param name="userManager">Manager providing user operations</param>
    /// <param name="templateRenderer">Renderer used to render macro templates</param>
    /// <param name="tokenService">Service used to generate security tokens for remote macro rendering</param>
    /// <param name="logger">Logger instance</param>
    public PluginService(IPermissionService permissionService,
        IPluginManager pluginManager,
        IPageManager pageManager,
        ISpaceManager spaceManager,
        IUserManager userManager,
        ITemplateRenderer templateRenderer,
        ISecurityTokenService tokenService,
        ILogger<PluginService> logger)
    {
        _permissionService = permissionService;
        _pluginManager = pluginManager;
        _pageManager = pageManager;
        _spaceManager = spaceManager;
        _userManager = userManager;
        _templateRenderer = templateRenderer;
        _tokenService = tokenService;
        _logger = logger;
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// Disables an enabled plugin
    /// </summary>
    /// <param name="pluginIdentifier">Unique identifier of the plugin to disable</param>
    /// <param name="userInfo">User information of the user performing the operation</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public Task DisablePlugin(string pluginIdentifier, UserInfo userInfo)
    {
        _logger.LogInformation("User {email} disable plugin with {id}", userInfo.Email, pluginIdentifier);
        return _pluginManager.Disable(pluginIdentifier);
    }

    /// <summary>
    /// Enables a previously disabled plugin
    /// </summary>
    /// <param name="pluginIdentifier">Unique identifier of the plugin to enable</param>
    /// <param name="userInfo">User information of the user performing the operation</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public Task EnablePlugin(string pluginIdentifier, UserInfo userInfo)
    {
        _logger.LogInformation("User {email} enable plugin with {id}", userInfo.Email, pluginIdentifier);
        return _pluginManager.Enable(pluginIdentifier);
    }

    /// <summary>
    /// Retrieves available macros with pagination support
    /// </summary>
    /// <param name="skip">Number of items to skip</param>
    /// <param name="take">Number of items to take</param>
    /// <returns>Array of available macro models</returns>
    public async Task<MacroModel[]> GetAvailableMacroses(int? skip = null, int? take = null)
    {
        var plugins = await _pluginManager.GetPlugins();
        var macroses = plugins.Where(x => !x.Disabled).SelectMany(x => x.Macros.Where(m => !m.Disabled).Select(m => m));
        if (skip is null && take is null)
            return macroses.Select(x => x.ToModelLite()).ToArray();
        return macroses.Select(x => x.ToModelLite())
            .Skip(skip ?? 0)
            .Take(take ?? 10)
            .ToArray();
    }

    /// <summary>
    /// Retrieves information about a specific macro
    /// </summary>
    /// <param name="macroIdentifier">Unique identifier of the macro</param>
    /// <returns>Macro model containing macro information</returns>
    public async Task<MacroModel> GetMacroInfo(string macroIdentifier)
    {
        var macro = await _pluginManager.GetMacro(macroIdentifier);
        if (macro == null)
            throw new PluginNotFoundException("Macros with identifier not found in plugins");
        return macro.ToModelLite();
    }

    /// <summary>
    /// Retrieves the state of a macro on a specific page
    /// </summary>
    /// <param name="pageId">Unique identifier of the page</param>
    /// <param name="macroIdOnPage">Unique identifier of the macro on the page</param>
    /// <param name="userInfo">User information of the user requesting the state</param>
    /// <returns>Current state of the macro on the specified page</returns>
    public async Task<MacroStateModel> GetMacroState(string pageId, string macroIdOnPage, UserInfo userInfo)
    {
        await _permissionService.EnsureAnonymousAllowed(userInfo);

        var page = await _pageManager.GetById(pageId) ?? throw new PageNotFoundException();
        var space = await _spaceManager.GetById(page.SpaceId) ?? throw new SpaceNotFoundException();
        await _permissionService.EnsureViewPermission(space.Key, userInfo);

        return (await _pluginManager.GetMacroState(pageId, macroIdOnPage)).ToModel();
    }

    /// <summary>
    /// Retrieves installed plugins with pagination support
    /// </summary>
    /// <param name="skip">Number of items to skip</param>
    /// <param name="take">Number of items to take</param>
    /// <returns>Array of installed plugin models</returns>
    public async Task<PluginModel[]> GetPlugins(int? skip = null, int? take = null)
    {
        var plugins = await _pluginManager.GetPlugins(skip, take);
        return plugins.Select(x => x.ToModel()).ToArray();
    }

    /// <summary>
    /// Installs a new plugin in the system
    /// </summary>
    /// <param name="model">The plugin model containing plugin details</param>
    /// <param name="userInfo">The user information of the user installing the plugin</param>
    /// <returns>The installed plugin model</returns>
    public async Task<PluginModel> InstallPlugin(PluginModel model, UserInfo userInfo)
    {
        await _pluginManager.InstallPlugin(model.ToEntity(), userInfo);
        var installedPlugin = await _pluginManager.GetPlugin(model.PluginIdentifier);
        return installedPlugin.ToModel();
    }

    /// <summary>
    /// Renders a macro on a specific page
    /// </summary>
    /// <param name="pageId">Unique identifier of the page containing the macro</param>
    /// <param name="macroIdOnPage">Unique identifier of the macro on the page</param>
    /// <param name="userRequest">Request containing user-specific rendering parameters</param>
    /// <param name="userInfo">User information of the user requesting the render</param>
    /// <returns>Response containing the rendered macro content</returns>
    public async Task<MacroRenderResponse> Render(string pageId, string macroIdOnPage, MacroRenderUserRequest userRequest, UserInfo userInfo)
    {
        await _permissionService.EnsureAnonymousAllowed(userInfo);
        
        var page = await _pageManager.GetById(pageId) ?? throw new PageNotFoundException();
        var space = await _spaceManager.GetById(page.SpaceId) ?? throw new SpaceNotFoundException();
        await _permissionService.EnsureViewPermission(space.Key, userInfo);

        var state = await _pluginManager.GetMacroState(pageId, macroIdOnPage);
        var macro = await _pluginManager.GetMacro(state.MacroIdentifier ?? userRequest.MacroIdentifier);
        if (macro == null)
            throw new MacroNotFoundException($"Macro {state.MacroIdentifier ?? userRequest.MacroIdentifier} not found");
        var plugin = await _pluginManager.GetPluginByMacroIdentifier(macro.MacroIdentifier);
        if(plugin.Disabled)
            throw new MacroNotFoundException($"Macro {state.MacroIdentifier ?? userRequest.MacroIdentifier} not found");
        foreach (var (key, value) in state.Params)
                if (!userRequest.Params.TryAdd(key, value))
                    userRequest.Params[key] = value;
        foreach (var (key, value) in macro.DefaultValues)
            if (!userRequest.Params.ContainsKey(key))
                userRequest.Params.Add(key, value);

        var user = userInfo is not null
            ? await _userManager.GetByEmail(userInfo.Email)
            : null;

        userRequest.Params.Add("MacroIdOnPage", macroIdOnPage);
        userRequest.Params.Add("PageId", page.Id);
        userRequest.Params.Add("PageName", page.Name);
        userRequest.Params.Add("SpaceKey", space.Key);
        userRequest.Params.Add("SpaceName", space.Name);
        userRequest.Params.Add("UserEmail", userInfo?.Email ?? string.Empty);
        userRequest.Params.Add("UserName", userInfo?.Name ?? string.Empty);
        userRequest.Params.Add("UserRole", user?.Role.ToString() ?? string.Empty);

        if (!string.IsNullOrEmpty(macro.RenderUrl))
            return await RenderRemoteMacro(plugin, macro, page, space, user, userRequest.Params);

        var renderResult = await _templateRenderer.Render(macro.Template, userRequest.Params.ToDictionary(x => x.Key, x => x.Value as object));
        return new MacroRenderResponse()
        {
            Html = renderResult
        };
    }

    /// <summary>
    /// Saves the state of a macro
    /// </summary>
    /// <param name="state">State model containing the new state</param>
    /// <param name="userInfo">User information of the user saving the state</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task SaveMacroState(MacroStateModel state, UserInfo userInfo)
    {
        var page = await _pageManager.GetById(state.PageId) ?? throw new PageNotFoundException();
        var space = await _spaceManager.GetById(page.SpaceId) ?? throw new SpaceNotFoundException();
        await _permissionService.EnsureEditPermission(space.Key, userInfo);

        await _pluginManager.CreateOrUpdateState(state.ToEntity());
    }

    /// <summary>
    /// Uninstalls a plugin from the system
    /// </summary>
    /// <param name="id">Unique identifier of the plugin to uninstall</param>
    /// <param name="userInfo">User information of the user performing the operation</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task UnInstallPlugin(string id, UserInfo userInfo)
    {
        var installedPlugin = await _pluginManager.GetPlugin(id);
        if (installedPlugin is null)
            throw new PluginNotFoundException();
        await _pluginManager.UnInstall(installedPlugin, userInfo); ;
    }
    
    
    private async Task<MacroRenderResponse> RenderRemoteMacro(Plugin plugin,
        Macro macro,
        Page page,
        Space space,
        User user,
        IDictionary<string, string> parameters)
    {
        var userToken = macro.SendUserToken && user is not null
            ? await _tokenService.GenerateAccessToken(user, TimeSpan.FromMinutes(15), true) 
            : string.Empty;
        var renderRequest = new MacroRenderRequest()
        {
            PluginIdentifier = plugin.PluginIdentifier,
            MacroIdentifier = macro.MacroIdentifier,
            RequestedBy = user?.ToModel(),
            PageId = page.Id,
            SpaceKey = space.Key,
            UserToken = userToken,
            Params = parameters
        };
        try
        {
            var response = await _httpClient.PostAsJsonAsync(macro.RenderUrl, renderRequest);
            return await response.Content.ReadFromJsonAsync<MacroRenderResponse>();
        }
        catch(Exception e)
        {
            _logger.LogError(e, "Error while rendering remote macro");
            throw new RemoteMacroRenderException($"Error while rendering remote macro: {e.Message}");
        }
    }
}