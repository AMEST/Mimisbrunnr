using Microsoft.Extensions.DependencyInjection;
using Mimisbrunnr.Web.Host.Services;
using Skidbladnir.Modules;

namespace Mimisbrunnr.DataImport.Confluence;

public class ConfluenceDataImportModule : Module
{
    public override void Configure(IServiceCollection services)
    {
        services.AddSingleton<IDataImportService, ConfluenceDataImportService>();
    }
}