using Riok.Mapperly.Abstractions;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Integration.Plugin;

namespace Mimisbrunnr.Web.Mapping;

[Mapper]
public static partial class PluginMapper
{
    public static partial PluginModel ToModel(this Mimisbrunnr.Wiki.Contracts.Plugin plugin);

    [MapperIgnoreSource(nameof(Macro.Disabled))]
    [UserMapping(Default = true)]
    public static partial MacroModel ToModel(this Macro macro);

    [MapperIgnoreSource(nameof(Macro.RenderUrl))]
    [MapperIgnoreTarget(nameof(MacroModel.RenderUrl))]
    [MapperIgnoreSource(nameof(Macro.Template))]
    [MapperIgnoreTarget(nameof(MacroModel.Template))]
    [MapperIgnoreSource(nameof(Macro.SendUserToken))]
    [MapperIgnoreTarget(nameof(MacroModel.SendUserToken))]
    [MapperIgnoreSource(nameof(Macro.Disabled))]
    public static partial MacroModel ToModelLite(this Macro macro);

    public static partial Mimisbrunnr.Wiki.Contracts.Plugin ToEntity(this PluginModel plugin);

    public static partial MacroState ToEntity(this MacroStateModel macroState);

    public static partial MacroStateModel ToModel(this MacroState macroState);
}