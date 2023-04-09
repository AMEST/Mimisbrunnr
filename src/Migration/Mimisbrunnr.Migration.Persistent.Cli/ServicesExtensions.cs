using Microsoft.Extensions.DependencyInjection;
using Mimisbrunnr.Persistent;
using Mimisbrunnr.Storage.MongoDb.Mappings;
using Mimisbrunnr.Wiki.Contracts;
using MongoDB.Bson.Serialization.Conventions;
using Skidbladnir.Repository.MongoDB;
using Skidbladnir.Storage.Abstractions;
using Skidbladnir.Storage.GridFS;
using Skidbladnir.Storage.LocalFileStorage;
using Skidbladnir.Storage.S3;
using Skidbladnir.Storage.WebDav;
using StorageType = Mimisbrunnr.Persistent.StorageType;

namespace Mimisbrunnr.Migration.Persistent.Cli;

public static class ServicesExtensions
{
    public static IServiceCollection AddFileStorage(this IServiceCollection services, PersistentModuleConfiguration configuration)
    {
        switch (configuration.Type)
        {
            case StorageType.Local:
                var attachPath = Path.Combine(configuration.Local.Path, "attachments");
                if (!Directory.Exists(attachPath))
                    Directory.CreateDirectory(attachPath);
                services
                    .AddLocalFsStorage(configuration.Local);
                break;
            case StorageType.GridFs:
                services
                    .AddGridFsStorage(configuration.GridFs);
                break;
            case StorageType.WebDav:
                services
                    .AddWebDavStorage(configuration.WebDav);
                break;
            case StorageType.S3:
                services
                    .AddS3Storage(configuration.S3);
                break;
        }

        return services;
    }

    public static IServiceCollection AddDataBase(this IServiceCollection services, string connectionString)
    {
        // Register conventions
        var pack = new ConventionPack
        {
            new IgnoreIfDefaultConvention(true),
            new IgnoreExtraElementsConvention(true),
        };

        ConventionRegistry.Register("Mimisbrunnr", pack, t => true); 
        return services.AddMongoDbContext(builder =>
            builder
                .UseConnectionString(connectionString)
                .AddEntity<Attachment, AttachmentMap>()
            );
    }

    public static IStorage GetStorage(this IServiceProvider provider, StorageType type)
    {
        switch (type)
        {
            case StorageType.Local:
                return provider.GetRequiredService<IStorage<LocalStorageInfo>>();
            case StorageType.GridFs:
                return provider.GetRequiredService<IStorage<GridFsStorageInfo>>();
            case StorageType.WebDav:
                return provider.GetRequiredService<IStorage<WebDavStorageInfo>>();
            case StorageType.S3:
                return provider.GetRequiredService<IStorage<S3StorageInfo>>();
            default:
                throw new ArgumentOutOfRangeException(nameof(type), "Unknown storage type");
        }
    }
}