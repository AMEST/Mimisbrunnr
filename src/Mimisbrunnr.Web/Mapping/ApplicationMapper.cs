using Mimisbrunnr.Web.Administration;
using Mimisbrunnr.Web.Infrastructure.Contracts;
using Mimisbrunnr.Web.Quickstart;
using Riok.Mapperly.Abstractions;

namespace Mimisbrunnr.Web.Mapping;

[Mapper]
public static partial class ApplicationMapper
{
    [MapperIgnoreTarget(nameof(ApplicationConfiguration.Id))]
    [MapperIgnoreTarget(nameof(ApplicationConfiguration.CustomCss))]
    [MapperIgnoreTarget(nameof(ApplicationConfiguration.CustomHomepageSpaceKey))]
    public static partial ApplicationConfiguration ToEntity(this QuickstartModel model);

    [MapperIgnoreSource(nameof(ApplicationConfiguration.Id))]
    public static partial ApplicationConfigurationModel ToModel(this ApplicationConfiguration applicationConfiguration);

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