namespace Mimisbrunnr.Web.Host.Configuration
{
    public class CachingConfiguration
    {
        public string RedisConnectionString { get; set; }

        public CachingType Type { get; set; }
    }

    public enum CachingType
    {
        Memory,
        MongoDb,
        Redis
    }
}