namespace Mimisbrunnr.Web.Host.Configuration;

internal class CachingConfiguration
{
    public string RedisConnectionString { get; set; }

    public CachingType Type { get; set; }
}

internal enum CachingType
{
    Memory,
    MongoDb,
    Redis
}