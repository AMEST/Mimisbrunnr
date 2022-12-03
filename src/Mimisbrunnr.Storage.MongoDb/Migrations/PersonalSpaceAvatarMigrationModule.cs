using DnsClient.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mimisbrunnr.Users;
using Mimisbrunnr.Wiki.Contracts;
using Skidbladnir.Modules;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Storage.MongoDb.Migrations;

public class PersonalSpaceAvatarMigrationModule : BackgroundModule
{
    public override async Task ExecuteAsync(IServiceProvider provider, CancellationToken cancellationToken = default)
    {
        var logger = provider.GetService<ILogger<PersonalSpaceAvatarMigrationModule>>();
        var spaceRepository = provider.GetService<IRepository<Space>>();
        var userManager = provider.GetService<IUserManager>();
        logger.LogInformation("Start fix personal space, for adding avatar from owner");
        try
        {
            var personalSpacesWithoutAvatar = await spaceRepository
                .GetAll()
                .Where(x => x.Type == SpaceType.Personal && string.IsNullOrEmpty(x.AvatarUrl))
                .ToArrayAsync();
            foreach (var space in personalSpacesWithoutAvatar)
            {
                var owner = await userManager.GetByEmail(space.Key);
                if (owner is null) continue;
                space.AvatarUrl = owner.AvatarUrl;
                await spaceRepository.Update(space, cancellationToken);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Personal spaces fixing failed. Can't add avatars from owners");
        }
    }
}