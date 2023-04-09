using DocoptNet;

namespace Mimisbrunnr.Migration.Persistent.Cli;

[DocoptArguments]
public partial class CliConfiguration
{
    const string Help = @"Mimisbrunnr Wiki persistent storage migration cli.

    Usage:
      mimisbrunnr-migration-persistent-cli.exe --connection-string=<MongoDb url> --from-storage-type=<Type> --to-storage-type=<Type> [--local-path=<path to files>] [--gridfs-connection-string=<MongoDB url>] [--webdav-address=<url to webdav>] [--webdav-username=<username>] [--webdav-password=<password>] [--s3-service-url=<url to s3>] [--s3-bucket=<bucket name>] [--s3-access-key=<access key>] [--s3-secret-key=<secret key>] [--only-absent]
      mimisbrunnr-migration-persistent-cli.exe (-h | --help)

    Options:
      --connection-string=<MongoDb url>             MongoDb Connection String
      --only-absent                                 Only absent files in target storage
      --from-storage-type=<Type>                    Persistent type: Local, GridFs, WebDav, S3
      --to-storage-type=<Type>                      Persistent type: Local, GridFs, WebDav, S3
      --local-path=<path to files>                  Local storage: Path in file system where stored files
      --gridfs-connection-string=<MongoDB url>      GridFs storage: MongoDb Connection String
      --webdav-address=<url to webdav>              WebDav storage: Url to WebDav server 
      --webdav-username=<username>                  WebDav storage: WebDav username
      --webdav-password=<password>                  WebDav storage: WebDav password
      --s3-service-url=<url to s3>                  S3 Storage: Url to s3 compatible service 
      --s3-bucket=<bucket name>                     S3 Storage: Bucket name
      --s3-access-key=<access key>                  S3 Storage: S3 access key
      --s3-secret-key=<secret key>                  S3 Storage: S3 secret key
      -h --help                     Show this screen.
";
}