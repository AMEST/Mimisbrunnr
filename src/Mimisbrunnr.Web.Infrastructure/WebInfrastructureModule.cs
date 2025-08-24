using Microsoft.Extensions.DependencyInjection;
using Skidbladnir.Modules;

namespace Mimisbrunnr.Web.Infrastructure;

public class WebInfrastructureModule : Module
{
    public override void Configure(IServiceCollection services)
    {
        services.AddSingleton<IApplicationConfigurationManager, ApplicationConfigurationManager>();
        services.AddSingleton<ITemplateRenderer, StubbleTemplateRenderer>();
    }
}