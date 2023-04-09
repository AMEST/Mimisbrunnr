using Microsoft.Extensions.DependencyInjection;
using Skidbladnir.Modules;
using Skidbladnir.Storage.Abstractions;
using Skidbladnir.Storage.GridFS;
using Skidbladnir.Storage.LocalFileStorage;
using Skidbladnir.Storage.S3;
using Skidbladnir.Storage.WebDav;

namespace Mimisbrunnr.Persistent
{
    public class PersistentModule : Module
    {
        public override void Configure(IServiceCollection services)
        {
            var configuration = Configuration.Get<PersistentModuleConfiguration>();
            switch (configuration.Type)
            {
                case StorageType.Local:
                    var attachPath = Path.Combine(configuration.Local.Path, "attachments");
                    if (!Directory.Exists(attachPath))
                        Directory.CreateDirectory(attachPath);
                    services
                        .AddLocalFsStorage(configuration.Local)
                        .AddSingleton<IStorage>(r => r.GetService<IStorage<LocalStorageInfo>>());
                    break;
                case StorageType.GridFs:
                    services
                        .AddGridFsStorage(configuration.GridFs)
                        .AddSingleton<IStorage>(r => r.GetService<IStorage<GridFsStorageInfo>>());
                    break;
                case StorageType.WebDav:
                    services
                        .AddWebDavStorage(configuration.WebDav)
                        .AddSingleton<IStorage>(r => r.GetService<IStorage<WebDavStorageInfo>>());
                    break;
                case StorageType.S3:
                    services
                        .AddS3Storage(configuration.S3)
                        .AddSingleton<IStorage>(r => r.GetService<IStorage<S3StorageInfo>>());
                    break;
            }
        }
    }
}