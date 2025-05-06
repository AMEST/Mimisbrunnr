using Mimisbrunnr.Wiki.Contracts;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Wiki.Services;

public interface IPluginService
{
    Task<Plugin[]> GetPlugins(int? skip = null, int? top = null);

    Task InstallPlugin(Plugin plugin, UserInfo userInfo);

    Task UnInstall(Plugin plugin);

    Task<MacroState> GetMacroState(string pageId, string macroUniqueId);

    Task<MacroState> CreateOrUpdateState(MacroState macroState);

    Task DeleteState(string pageId, string macroUniqueId);
    Task DeleteAllStateInPage(string pageId);
}