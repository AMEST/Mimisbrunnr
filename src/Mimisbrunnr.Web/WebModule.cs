using Microsoft.Extensions.DependencyInjection;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Administration;
using Mimisbrunnr.Web.Authentication.Account;
using Mimisbrunnr.Web.Customization;
using Mimisbrunnr.Web.Feed;
using Mimisbrunnr.Web.Group;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Web.Quickstart;
using Mimisbrunnr.Web.Search;
using Mimisbrunnr.Web.Services;
using Mimisbrunnr.Web.User;
using Mimisbrunnr.Web.Wiki;
using Mimisbrunnr.Wiki.Contracts;
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
        services.AddSingleton<ICustomizationService, CustomizationService>();
        services.AddSingleton<IDraftService, DraftService>();
        services.AddSingleton<ITokenService, TokenService>();
    }
}