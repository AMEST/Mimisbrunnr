using Microsoft.Extensions.DependencyInjection;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Administration;
using Mimisbrunnr.Web.Feed;
using Mimisbrunnr.Web.Group;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Quickstart;
using Mimisbrunnr.Web.Search;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Web.User;
using Mimisbrunnr.Web.Wiki;
using Mimisbrunnr.Web.Wiki.Import;
using Skidbladnir.Modules;

namespace Mimisbrunnr.Web;

public class WebModule : Module
{
    public override Type[] DependsModules => new[] { typeof(WebInfrastructureModule), typeof(UsersModule) };

    public override void Configure(IServiceCollection services)
    {
        services.AddSingleton<IQuickstartService, QuickstartService>();
        services.AddSingleton<ISpaceService, SpaceService>();
        services.AddSingleton<ISpaceDisplayService>(r => (ISpaceDisplayService)r.GetService<ISpaceService>());
        services.AddSingleton<IPageService, PageService>();
        services.AddSingleton<IUserService, UserService>();
        services.AddSingleton<IFeedService, FeedService>();
        services.AddSingleton<IAttachmentService, AttachmentService>();
        services.AddSingleton<ISearchService, SearchService>();
        services.AddSingleton<IGroupService, GroupService>();
        services.AddSingleton<IApplicationConfigurationService, ApplicationConfigurationService>();
    }
}