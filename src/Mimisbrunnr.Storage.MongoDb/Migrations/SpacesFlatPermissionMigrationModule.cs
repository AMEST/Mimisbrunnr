using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mimisbrunnr.Wiki.Contracts;
using Skidbladnir.Modules;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Storage.MongoDb.Migrations
{
    public class SpacesFlatPermissionMigrationModule : BackgroundModule
    {
        private const int BatchSize = 100;

        public override async Task ExecuteAsync(IServiceProvider provider, CancellationToken cancellationToken = default)
        {
            var logger = provider.GetService<ILogger<SpacesFlatPermissionMigrationModule>>();
            var spaceRepository = provider.GetService<IRepository<Space>>();
            
            logger.LogInformation("Permissions migration started");

            var page = 0;
            var spaces = await GetBatch(spaceRepository, page++);
            while (spaces.Length > 0)
            {
                foreach( var space in spaces){
                    var previousCount = space.PermissionsFlat == null ? 0 : space.PermissionsFlat.Length;
                    space.UpdatePermissions();
                    if(previousCount != space.PermissionsFlat.Length)
                        await spaceRepository.Update(space);
                }
                spaces = await GetBatch(spaceRepository, page++);
            }
            logger.LogInformation("Permissions migration completed");

        }

        private static async Task<Space[]> GetBatch(IRepository<Space> repository, int page)
        {
            return await repository.GetAll().Take(BatchSize).Skip(BatchSize * page).ToArrayAsync();
        }

    }
}