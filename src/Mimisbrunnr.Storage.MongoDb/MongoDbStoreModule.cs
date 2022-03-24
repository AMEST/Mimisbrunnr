using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mimisbrunner.Users;
using Mimisbrunnr.Storage.MongoDb.Mappings;
using Mimisbrunnr.Web.Infrastructure.Contracts;
using Mimisbrunnr.Wiki.Contracts;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Skidbladnir.Modules;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb;

public class MongoDbStoreModule : RunnableModule
{
    public override void Configure(IServiceCollection services)
    {
        // Register conventions
        var pack = new ConventionPack
        {
            new IgnoreIfDefaultConvention(true),
            new IgnoreExtraElementsConvention(true),
        };

        ConventionRegistry.Register("Influunt", pack, t => true);
        var configuration = Configuration.Get<MongoDbStoreModuleConfiguration>();
        services.AddMongoDbContext(builder => 
            builder
                .UseConnectionString(configuration.ConnectionString)
                .AddEntity<ApplicationConfiguration,ApplicationConfigurationMap>()
                .AddEntity<User,UserMap>()
                .AddEntity<Group, GroupMap>()
                .AddEntity<UserGroup, UserGroupMap>()
                .AddEntity<Space, SpaceMap>()
                .AddEntity<Page, PageMap>()
        );
    }

    public override async Task StartAsync(IServiceProvider provider, CancellationToken cancellationToken)
    {
        var logger = provider.GetService<ILogger<MongoDbStoreModule>>();
        var baseMongoContext = provider.GetService<BaseMongoDbContext>();
        try
        {
            await CreateUserIndexes(baseMongoContext);
            await CreateGroupIndexes(baseMongoContext);
            await CreateSpaceIndexes(baseMongoContext);
            await CreatePageIndexes(baseMongoContext);
        }
        catch (Exception e)
        {
            logger?.LogError(e, "Can't create indexes");
        }
    }

    private async Task CreateUserIndexes(IMongoDbContext mongoContext)
    {
        var collection = mongoContext.GetCollection<User>();
        var emailKeyDefinition = Builders<User>.IndexKeys.Ascending(x => x.Email);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<User>(emailKeyDefinition, new CreateIndexOptions()
        {
            Unique = true,
            Background = true
        }));
        var nameKeyDefinition = Builders<User>.IndexKeys.Ascending(x => x.Name);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<User>(nameKeyDefinition, new CreateIndexOptions()
        {
            Background = true
        }));
    }
    private async Task CreateGroupIndexes(IMongoDbContext mongoContext)
    {
        var collection = mongoContext.GetCollection<Group>();
        var nameKeyDefinition = Builders<Group>.IndexKeys.Ascending(x => x.Name);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<Group>(nameKeyDefinition, new CreateIndexOptions()
        {
            Unique = true,
            Background = true
        }));
    }
    
    private async Task CreateSpaceIndexes(IMongoDbContext mongoContext)
    {
        var collection = mongoContext.GetCollection<Space>();
        var keyDefinition = Builders<Space>.IndexKeys.Ascending(x => x.Key);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<Space>(keyDefinition, new CreateIndexOptions()
        {
            Unique = true,
            Background = true
        }));
        var nameKeyDefinition = Builders<Space>.IndexKeys.Ascending(x => x.Name);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<Space>(nameKeyDefinition, new CreateIndexOptions()
        {
            Background = true
        }));
    }
    
    private async Task CreatePageIndexes(IMongoDbContext mongoContext)
    {
        var collection = mongoContext.GetCollection<Page>();
        var spaceIdkeyDefinition = Builders<Page>.IndexKeys.Ascending(x => x.SpaceId);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<Page>(spaceIdkeyDefinition, new CreateIndexOptions()
        {
            Background = true
        }));
        var parentPageIdkeyDefinition = Builders<Page>.IndexKeys.Ascending(x => x.ParentId);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<Page>(parentPageIdkeyDefinition, new CreateIndexOptions()
        {
            Background = true
        }));

        var nameKeyDefinition = Builders<Page>.IndexKeys.Ascending(x => x.Name);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<Page>(nameKeyDefinition, new CreateIndexOptions()
        {
            Background = true
        }));
    }
}