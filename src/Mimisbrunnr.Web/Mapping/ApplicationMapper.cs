using Mimisbrunnr.Web.Administration;
using Mimisbrunnr.Web.Infrastructure.Contracts;
using Mimisbrunnr.Web.Quickstart;
using Riok.Mapperly.Abstractions;

namespace Mimisbrunnr.Web.Mapping;

/// <summary>
/// Maps application configuration entities to and from their web models
/// </summary>
[Mapper]
public static partial class ApplicationMapper
{
    /// <summary>
    /// Maps a quickstart model to an application configuration entity
    /// </summary>
    /// <param name="model">Quickstart model to map</param>
    /// <returns>The mapped application configuration entity</returns>
    [MapperIgnoreTarget(nameof(ApplicationConfiguration.Id))]
    [MapperIgnoreTarget(nameof(ApplicationConfiguration.CustomCss))]
    [MapperIgnoreTarget(nameof(ApplicationConfiguration.CustomHomepageSpaceKey))]
    public static partial ApplicationConfiguration ToEntity(this QuickstartModel model);

    /// <summary>
    /// Maps an application configuration entity to an application configuration model
    /// </summary>
    /// <param name="applicationConfiguration">Application configuration entity to map</param>
    /// <returns>The mapped application configuration model</returns>
    [MapperIgnoreSource(nameof(ApplicationConfiguration.Id))]
    public static partial ApplicationConfigurationModel ToModel(this ApplicationConfiguration applicationConfiguration);

    /// <summary>
    /// Maps an application configuration entity to a quickstart model
    /// </summary>
    /// <param name="model">Application configuration entity to map</param>
    /// <returns>The mapped quickstart model</returns>
    public static QuickstartModel ToQuickStartModel(this ApplicationConfiguration model)
    {
        return new QuickstartModel()
        {
            Title = model?.Title ?? "Mimisbrunnr",
            AllowAnonymous = model?.AllowAnonymous ?? false,
            AllowHtml = model?.AllowHtml ?? true,
            SwaggerEnabled = model?.SwaggerEnabled ?? false,
            CustomHomepageEnabled = model?.CustomHomepageEnabled ?? false
        };
    }

}