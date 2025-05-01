using Mimisbrunnr.Wiki.Contracts;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Wiki.Services;

internal class PluginService : IPluginService
{
    private readonly IRepository<Plugin> _pluginRepository;
    private readonly IRepository<MacroState> _macroStateRepository;

    public PluginService(IRepository<Plugin> pluginRepository,
        IRepository<MacroState> macroStateRepository
    )
    {
        _pluginRepository = pluginRepository;
        _macroStateRepository = macroStateRepository;
    }

    public async Task<MacroState> CreateOrUpdateState(MacroState macroState)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(macroState?.MacroIdentifierOnPage, nameof(macroState.MacroIdentifierOnPage));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(macroState?.PageId, nameof(macroState.PageId));
        MacroState state;

        if (!string.IsNullOrWhiteSpace(macroState.Id))
        {
            state = await _macroStateRepository.GetAll().FirstOrDefaultAsync(x => x.Id == macroState.Id);
            if (macroState.PageId != state.PageId || macroState.MacroIdentifierOnPage != state.MacroIdentifierOnPage)
                throw new InvalidOperationException("PageId and UniqueId in store not equals in updating state by Id");
            state.Params = macroState.Params;
            await _macroStateRepository.Update(state);
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
        throw new NotImplementedException();
    }

    public async Task UnInstall(Plugin plugin)
    {
        var macroIdentifiers = plugin.Macros.Select(x => x.MacroIdentifier).ToArray();
        var states = await _macroStateRepository.GetAll().Where(x => macroIdentifiers.Contains(x.MacroIdentifier)).ToArrayAsync();
        foreach (var state in states)
            await _macroStateRepository.Delete(state);

        await _pluginRepository.Delete(plugin);
    }

}
