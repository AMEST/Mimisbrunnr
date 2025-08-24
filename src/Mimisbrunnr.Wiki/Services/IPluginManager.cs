using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Wiki.Services;

public interface IPluginManager
{
    Task<Plugin[]> GetPlugins(int? skip = null, int? top = null);
    Task<Plugin> GetPlugin(string pluginIdentifier);
    Task InstallPlugin(Plugin plugin, UserInfo userInfo);
    Task UnInstall(Plugin plugin, UserInfo userInfo);
    Task Enable(string pluginIdentifier);
    Task Disable(string pluginIdentifier);
    Task<Macro> GetMacro(string macroIdentifier);
    Task<MacroState> GetMacroState(string pageId, string macroUniqueId);
    Task<MacroState> CreateOrUpdateState(MacroState macroState);
    Task DeleteState(string pageId, string macroUniqueId);
    Task DeleteAllStateInPage(string pageId);
}