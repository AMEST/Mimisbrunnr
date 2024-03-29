﻿using Microsoft.Extensions.DependencyInjection;
using Mimisbrunnr.Wiki.Services;
using Skidbladnir.Modules;

namespace Mimisbrunnr.Wiki;

public class WikiModule : Module
{
    public override void Configure(IServiceCollection services)
    {
        services.AddSingleton<IPageManager, PageManager>()
            .AddSingleton<ISpaceManager, SpaceManager>()
            .AddSingleton<IFeedManager, FeedManager>()
            .AddSingleton<IAttachmentManager, AttachmentManager>()
            .AddSingleton<ISpaceSearcher, SpaceManager>()
            .AddSingleton<IDraftManager, DraftManager>()
            .AddSingleton<ICommentManager, CommentManager>();
    }
}