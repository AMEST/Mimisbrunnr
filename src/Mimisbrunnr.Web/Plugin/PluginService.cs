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

    public Task DisablePlugin(string pluginIdentifier, UserInfo userInfo)
    {
        _logger.LogInformation("User {email} disable plugin with {id}", userInfo.Email, pluginIdentifier);
        return _pluginManager.Disable(pluginIdentifier);
    }

    public Task EnablePlugin(string pluginIdentifier, UserInfo userInfo)
    {
        _logger.LogInformation("User {email} enable plugin with {id}", userInfo.Email, pluginIdentifier);
        return _pluginManager.Enable(pluginIdentifier);
    }

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

    public async Task<MacroModel> GetMacroInfo(string macroIdentifier)
    {
        var macro = await _pluginManager.GetMacro(macroIdentifier);
        if (macro == null)
            throw new PluginNotFoundException("Macros with identifier not found in plugins");
        return macro.ToModelLite();
    }

    public async Task<MacroStateModel> GetMacroState(string pageId, string macroIdOnPage, UserInfo userInfo)
    {
        await _permissionService.EnsureAnonymousAllowed(userInfo);

        var page = await _pageManager.GetById(pageId) ?? throw new PageNotFoundException();
        var space = await _spaceManager.GetById(page.SpaceId) ?? throw new SpaceNotFoundException();
        await _permissionService.EnsureViewPermission(space.Key, userInfo);

        return (await _pluginManager.GetMacroState(pageId, macroIdOnPage)).ToModel();
    }

    public async Task<PluginModel[]> GetPlugins(int? skip = null, int? take = null)
    {
        var plugins = await _pluginManager.GetPlugins(skip, take);
        return plugins.Select(x => x.ToModel()).ToArray();
    }

    public async Task<PluginModel> InstallPlugin(PluginModel model, UserInfo userInfo)
    {
        await _pluginManager.InstallPlugin(model.ToEntity(), userInfo);
        var installedPlugin = await _pluginManager.GetPlugin(model.PluginIdentifier);
        return installedPlugin.ToModel();
    }

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

    public async Task SaveMacroState(MacroStateModel state, UserInfo userInfo)
    {
        var page = await _pageManager.GetById(state.PageId) ?? throw new PageNotFoundException();
        var space = await _spaceManager.GetById(page.SpaceId) ?? throw new SpaceNotFoundException();
        await _permissionService.EnsureEditPermission(space.Key, userInfo);

        await _pluginManager.CreateOrUpdateState(state.ToEntity());
    }

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