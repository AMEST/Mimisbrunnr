using Microsoft.Extensions.Logging;
using Mimisbrunnr.Wiki.Contracts;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Wiki.Services;

internal class PluginManager : IPluginManager
{
    private readonly IRepository<Plugin> _pluginRepository;
    private readonly IRepository<MacroState> _macroStateRepository;
    private readonly ILogger<PluginManager> _logger;

    public PluginManager(IRepository<Plugin> pluginRepository,
        IRepository<MacroState> macroStateRepository,
        ILogger<PluginManager> logger
    )
    {
        _pluginRepository = pluginRepository;
        _macroStateRepository = macroStateRepository;
        _logger = logger;
    }

    public async Task<MacroState> CreateOrUpdateState(MacroState macroState)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(macroState?.MacroIdentifierOnPage, nameof(macroState.MacroIdentifierOnPage));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(macroState?.PageId, nameof(macroState.PageId));
        MacroState state;

        if (!string.IsNullOrWhiteSpace(macroState.Id))
        {
            state = await _macroStateRepository.GetAll().FirstOrDefaultAsync(x => x.Id == macroState.Id);
            if (state is null)
                throw new InvalidOperationException("State with specified Id not found");
            if (macroState.PageId != state?.PageId || macroState.MacroIdentifierOnPage != state?.MacroIdentifierOnPage)
                throw new InvalidOperationException("PageId and UniqueId in store not equals in updating state by Id");
            state.Params = macroState.Params;
            await _macroStateRepository.Update(state);
            return state;
        }

        state = await _macroStateRepository.GetAll().FirstOrDefaultAsync(x => x.PageId == macroState.PageId && x.MacroIdentifierOnPage == macroState.MacroIdentifierOnPage);
        if (state is null)
        {
            await _macroStateRepository.Create(macroState);
            return macroState;
        }
        state.Params = macroState.Params;
        await _macroStateRepository.Update(state);
        return state;
    }

    public async Task DeleteAllStateInPage(string pageId)
    {
        await _macroStateRepository.DeleteAll(x => x.PageId == pageId);
    }

    public async Task DeleteState(string pageId, string macroUniqueId)
    {
        var state = await _macroStateRepository.GetAll()
            .FirstOrDefaultAsync(x => x.PageId == pageId && x.MacroIdentifierOnPage == macroUniqueId);
        if (state is null)
            return;
        await _macroStateRepository.Delete(state);
    }

    public async Task Disable(string pluginIdentifier)
    {
        var plugin = await _pluginRepository.GetAll().FirstOrDefaultAsync(x => x.PluginIdentifier == pluginIdentifier);
        if (plugin is null)
            return;
        plugin.Disabled = true;
        await _pluginRepository.Update(plugin);
    }

    public async Task Enable(string pluginIdentifier)
    {
        var plugin = await _pluginRepository.GetAll().FirstOrDefaultAsync(x => x.PluginIdentifier == pluginIdentifier);
        if (plugin is null)
            return;
        plugin.Disabled = false;
        await _pluginRepository.Update(plugin);
    }

    public async Task<Macro> GetMacro(string macroIdentifier)
    {
        var pluginWithMacro = await _pluginRepository.GetAll().FirstOrDefaultAsync(x => x.Macros.Any(m => m.MacroIdentifier == macroIdentifier));
        if (pluginWithMacro is null)
            return null;
        return pluginWithMacro.Macros.FirstOrDefault(x => x.MacroIdentifier == macroIdentifier);
    }

    public async Task<MacroState> GetMacroState(string pageId, string macroUniqueId)
    {
        var state = await _macroStateRepository.GetAll()
            .FirstOrDefaultAsync(x => x.PageId == pageId && x.MacroIdentifierOnPage == macroUniqueId);
        if (state is not null)
            return state;
        return new MacroState()
        {
            PageId = pageId,
            MacroIdentifierOnPage = macroUniqueId
        };
    }

    public Task<Plugin> GetPlugin(string pluginIdentifier)
    {
        return _pluginRepository.GetAll().FirstOrDefaultAsync(x => x.PluginIdentifier == pluginIdentifier);
    }

    public async Task<Plugin[]> GetPlugins(int? skip = null, int? top = null)
    {
        var pluginQuery = _pluginRepository.GetAll();
        if (skip.HasValue)
            pluginQuery = pluginQuery.Skip(skip.Value);
        if (top.HasValue)
            pluginQuery = pluginQuery.Take(top.Value);
        return await pluginQuery.ToArrayAsync();
    }

    public async Task InstallPlugin(Plugin plugin, UserInfo userInfo)
    {
        using var _ = _logger.BeginScope("Installing plugin `{pluginIdentifier}` by user {userEmail}", plugin.PluginIdentifier, userInfo.Email);
        var pluginInDatabase = await _pluginRepository.GetAll().FirstOrDefaultAsync(x => x.PluginIdentifier == plugin.PluginIdentifier);
        if (pluginInDatabase is not null && pluginInDatabase.Version == plugin.Version)
        {
            _logger.LogInformation("Plugin `{pluginName}` updating skipped. Version equals {oldVersion}=={newVersion}", plugin.Name, pluginInDatabase.Version, plugin.Version);
            return;
        }

        if (pluginInDatabase is null)
        {
            plugin.InstalledBy = userInfo;
            plugin.Installation = DateTime.UtcNow;
            await _pluginRepository.Create(plugin);
            _logger.LogInformation("Plugin `{pluginName}` with version {version} installed successful", plugin.Name, plugin.Version);
            return;
        }

        pluginInDatabase.InstalledBy = userInfo;
        pluginInDatabase.Installation = DateTime.UtcNow;
        pluginInDatabase.Version = plugin.Version;
        pluginInDatabase.Macros = plugin.Macros;
        await _pluginRepository.Update(pluginInDatabase);
        _logger.LogInformation("Plugin `{pluginName}` with version {version} updated successful", plugin.Name, plugin.Version);
    }

    public async Task UnInstall(Plugin plugin, UserInfo userInfo)
    {
        using var _ = _logger.BeginScope("Uninstalling plugin `{pluginIdentifier}` by user {userEmail}", plugin.PluginIdentifier, userInfo.Email);
        var macroIdentifiers = plugin.Macros.Select(x => x.MacroIdentifier).ToArray();
        var states = await _macroStateRepository.GetAll().Where(x => macroIdentifiers.Contains(x.MacroIdentifier)).ToArrayAsync();
        foreach (var state in states)
            await _macroStateRepository.Delete(state);

        await _pluginRepository.Delete(plugin);
        _logger.LogInformation("Plugin `{pluginName}` with version {version} successful uninstalled with deleting all macro states", plugin.Name, plugin.Version);
    }

}
