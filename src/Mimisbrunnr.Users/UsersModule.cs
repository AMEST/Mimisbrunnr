﻿using Microsoft.Extensions.DependencyInjection;
using Mimisbrunnr.Users.Services;
using Skidbladnir.Modules;

namespace Mimisbrunnr.Users;

public class UsersModule : Module
{
    public override void Configure(IServiceCollection services)
    {
        services.AddSingleton<IUserManager, UserManager>()
            .AddSingleton<IUserGroupManager, UserGroupManager>()
            .AddSingleton<IUserSearcher, UserManager>();
    }
}