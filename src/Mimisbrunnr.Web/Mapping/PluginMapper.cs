using Riok.Mapperly.Abstractions;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Integration.Plugin;

namespace Mimisbrunnr.Web.Mapping;

/// <summary>
/// Maps plugin and macro entities to and from their web models
/// </summary>
[Mapper]
public static partial class PluginMapper
{
    /// <summary>
    /// Maps a plugin entity to a plugin model
    /// </summary>
    /// <param name="plugin">Plugin entity to map</param>
    /// <returns>The mapped plugin model</returns>
    public static partial PluginModel ToModel(this Mimisbrunnr.Wiki.Contracts.Plugin plugin);

    /// <summary>
    /// Maps a macro to a macro model
    /// </summary>
    /// <param name="macro">Macro to map</param>
    /// <returns>The mapped macro model</returns>
    [MapperIgnoreSource(nameof(Macro.Disabled))]
    [UserMapping(Default = true)]
    public static partial MacroModel ToModel(this Macro macro);

    /// <summary>
    /// Maps a macro to a lightweight macro model, omitting render related fields
    /// </summary>
    /// <param name="macro">Macro to map</param>
    /// <returns>The mapped lightweight macro model</returns>
    [MapperIgnoreSource(nameof(Macro.RenderUrl))]
    [MapperIgnoreTarget(nameof(MacroModel.RenderUrl))]
    [MapperIgnoreSource(nameof(Macro.Template))]
    [MapperIgnoreTarget(nameof(MacroModel.Template))]
    [MapperIgnoreSource(nameof(Macro.SendUserToken))]
    [MapperIgnoreTarget(nameof(MacroModel.SendUserToken))]
    [MapperIgnoreSource(nameof(Macro.Disabled))]
    public static partial MacroModel ToModelLite(this Macro macro);

    /// <summary>
    /// Maps a plugin model to a plugin entity
    /// </summary>
    /// <param name="plugin">Plugin model to map</param>
    /// <returns>The mapped plugin entity</returns>
    public static partial Mimisbrunnr.Wiki.Contracts.Plugin ToEntity(this PluginModel plugin);

    /// <summary>
    /// Maps a macro state model to a macro state entity
    /// </summary>
    /// <param name="macroState">Macro state model to map</param>
    /// <returns>The mapped macro state entity</returns>
    public static partial MacroState ToEntity(this MacroStateModel macroState);

    /// <summary>
    /// Maps a macro state entity to a macro state model
    /// </summary>
    /// <param name="macroState">Macro state entity to map</param>
    /// <returns>The mapped macro state model</returns>
    public static partial MacroStateModel ToModel(this MacroState macroState);
}