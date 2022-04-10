using Microsoft.Extensions.DependencyInjection;
using Mimisbrunner.Users;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Quickstart;
using Mimisbrunnr.Web.User;
using Mimisbrunnr.Web.Wiki;
using Skidbladnir.Modules;

namespace Mimisbrunnr.Web;

public class WebModule : Module
{
    public override Type[] DependsModules => new[] { typeof(WebInfrastructureModule), typeof(UsersModule) };

    public override void Configure(IServiceCollection services)
    {
        services.AddSingleton<IQuickstartService, QuickstartService>();
        services.AddSingleton<ISpaceService, SpaceService>();
        services.AddSingleton<IPageService, PageService>();
        services.AddSingleton<IUserService, UserService>();
    }
}