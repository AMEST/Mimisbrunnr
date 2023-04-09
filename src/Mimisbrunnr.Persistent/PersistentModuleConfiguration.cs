using Skidbladnir.Storage.GridFS;
using Skidbladnir.Storage.LocalFileStorage;
using Skidbladnir.Storage.S3;
using Skidbladnir.Storage.WebDav;

namespace Mimisbrunnr.Persistent;

public class PersistentModuleConfiguration
{
    public StorageType Type { get; set; }

    public WebDavStorageConfiguration WebDav { get; set; }

    public GridFsStorageConfiguration GridFs { get; set; }

    public LocalFsStorageConfiguration Local { get; set; }

    public S3StorageConfiguration S3 { get; set; }
}
