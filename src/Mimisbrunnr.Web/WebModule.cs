using Microsoft.Extensions.DependencyInjection;
using Mimisbrunner.Users;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Quickstart;
using Skidbladnir.Modules;

namespace Mimisbrunnr.Web;

public class WebModule : Module
{
    public override Type[] DependsModules => new[] { typeof(WebInfrastructureModule), typeof(UsersModule) };

    public override void Configure(IServiceCollection services)
    {
        services.AddSingleton<IQuickstartService, QuickstartService>();
    }
}