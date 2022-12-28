using Mimisbrunnr.Web.Administration;
using Mimisbrunnr.Web.Infrastructure.Contracts;
using Mimisbrunnr.Web.Quickstart;
using Riok.Mapperly.Abstractions;

namespace Mimisbrunnr.Web.Mapping;

[Mapper]
public partial class ApplicationMapper
{
    public static ApplicationMapper Instance { get; } = new ApplicationMapper();

    public partial ApplicationConfiguration ToEntity(QuickstartModel model);

    public partial ApplicationConfigurationModel ToModel(ApplicationConfiguration applicationConfiguration);

    public QuickstartModel ToQuickStartModel(ApplicationConfiguration model)
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