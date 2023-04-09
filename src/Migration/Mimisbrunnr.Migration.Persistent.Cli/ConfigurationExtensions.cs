using Mimisbrunnr.Persistent;
using Skidbladnir.Storage.GridFS;
using Skidbladnir.Storage.LocalFileStorage;
using Skidbladnir.Storage.S3;
using Skidbladnir.Storage.WebDav;

namespace Mimisbrunnr.Migration.Persistent.Cli;

public static class ConfigurationExtensions
{
    public static StorageType GetFromStorageType(this CliConfiguration configuration)
    {
        return Enum.Parse<StorageType>(configuration.OptFromStorageType, true);
    }

    public static StorageType GetToStorageType(this CliConfiguration configuration)
    {
        return Enum.Parse<StorageType>(configuration.OptToStorageType, true);
    }

    public static PersistentModuleConfiguration GetPersistentModuleConfiguration(this CliConfiguration configuration, StorageType type)
    {
        var persistentConfiguration = new PersistentModuleConfiguration
        {
            Type = type
        };
        switch (type){
            case StorageType.Local:
                persistentConfiguration.Local = new LocalFsStorageConfiguration{
                    Path = configuration.OptLocalPath
                };
                break;
            case StorageType.GridFs:
                persistentConfiguration.GridFs = new GridFsStorageConfiguration{
                    ConnectionString = configuration.OptGridfsConnectionString?.Replace("\"","")
                };
                break;
            case StorageType.WebDav:
                persistentConfiguration.WebDav = new WebDavStorageConfiguration{
                    Address = configuration.OptWebdavAddress,
                    Username = configuration.OptWebdavUsername,
                    Password = configuration.OptWebdavPassword
                };
                break;
            case StorageType.S3:
                persistentConfiguration.S3 = new S3StorageConfiguration{
                    ServiceUrl = configuration.OptS3ServiceUrl,
                    Bucket = configuration.OptS3Bucket,
                    AccessKey = configuration.OptS3AccessKey,
                    SecretKey = configuration.OptS3SecretKey
                };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), "Unknown storage type");
        }
        return persistentConfiguration;
    }
}