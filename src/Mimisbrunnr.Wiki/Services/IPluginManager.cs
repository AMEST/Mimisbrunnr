using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Wiki.Services;

public interface IPluginManager
{
    Task<Plugin[]> GetPlugins(int? skip = null, int? top = null);
    Task InstallPlugin(Plugin plugin, UserInfo userInfo);
    Task UnInstall(Plugin plugin);
    Task Enable(string id);
    Task Disable(string id);
    Task<MacroState> GetMacroState(string pageId, string macroUniqueId);
    Task<MacroState> CreateOrUpdateState(MacroState macroState);
    Task DeleteState(string pageId, string macroUniqueId);
    Task DeleteAllStateInPage(string pageId);
}