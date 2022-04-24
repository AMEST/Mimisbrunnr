using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mimisbrunnr.Users;
using Mimisbrunnr.Storage.MongoDb.Mappings;
using Mimisbrunnr.Web.Infrastructure.Contracts;
using Mimisbrunnr.Wiki.Contracts;
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

        ConventionRegistry.Register("Mimisbrunnr", pack, t => true);
        var configuration = Configuration.Get<MongoDbStoreModuleConfiguration>();
        services.AddMongoDbContext(builder =>
            builder
                .UseConnectionString(configuration.ConnectionString)
                .AddEntity<ApplicationConfiguration, ApplicationConfigurationMap>()
                .AddEntity<User, UserMap>()
                .AddEntity<Group, GroupMap>()
                .AddEntity<UserGroup, UserGroupMap>()
                .AddEntity<Space, SpaceMap>()
                .AddEntity<Page, PageMap>()
                .AddEntity<PageUpdateEvent, PageUpdateEventMap>()
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
            await CreateUserGroupIndexes(baseMongoContext);
            await CreateSpaceIndexes(baseMongoContext);
            await CreatePageIndexes(baseMongoContext);
            await CreatePageUpdatesIndexes(baseMongoContext);
        }
        catch (Exception e)
        {
            logger?.LogError(e, "Can't create indexes");
        }
    }

    private static async Task CreateUserIndexes(IMongoDbContext mongoContext)
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
    private static async Task CreateGroupIndexes(IMongoDbContext mongoContext)
    {
        var collection = mongoContext.GetCollection<Group>();
        var nameKeyDefinition = Builders<Group>.IndexKeys.Ascending(x => x.Name);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<Group>(nameKeyDefinition, new CreateIndexOptions()
        {
            Unique = true,
            Background = true
        }));
    }

    private static async Task CreateUserGroupIndexes(IMongoDbContext mongoContext)
    {
        var collection = mongoContext.GetCollection<UserGroup>();
        var userGroupsKeDefinition = Builders<UserGroup>.IndexKeys.Ascending(x => x.UserId);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<UserGroup>(userGroupsKeDefinition, new CreateIndexOptions()
        {
            Background = true
        }));
        var userInGroupKeDefinition = Builders<UserGroup>.IndexKeys.Ascending(x => x.UserId).Ascending(x => x.GroupId);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<UserGroup>(userInGroupKeDefinition, new CreateIndexOptions()
        {
            Background = true
        }));
        var groupIdKeyDefinition = Builders<UserGroup>.IndexKeys.Ascending(x => x.GroupId);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<UserGroup>(groupIdKeyDefinition, new CreateIndexOptions()
        {
            Background = true
        }));
    }

    private static async Task CreateSpaceIndexes(IMongoDbContext mongoContext)
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

        var personalSpaceSearch = Builders<Space>.IndexKeys.Ascending(x => x.Type).Ascending(x => x.Permissions);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<Space>(personalSpaceSearch, new CreateIndexOptions()
        {
            Background = true
        }));

        var nameSpaceFullTextSearch = Builders<Space>.IndexKeys.Text(x => x.Name);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<Space>(nameSpaceFullTextSearch, new CreateIndexOptions()
        {
            Background = true
        }));
    }

    private static async Task CreatePageIndexes(IMongoDbContext mongoContext)
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

        var contentTitleFullTextSearch = Builders<Page>.IndexKeys.Text(x => x.Name).Text(x => x.Content);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<Page>(contentTitleFullTextSearch, new CreateIndexOptions()
        {
            Background = true
        }));
    }

    private static async Task CreatePageUpdatesIndexes(IMongoDbContext mongoContext)
    {
        var collection = mongoContext.GetCollection<PageUpdateEvent>();

        var dateDefinition = Builders<PageUpdateEvent>.IndexKeys.Descending(x => x.Updated);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<PageUpdateEvent>(dateDefinition, new CreateIndexOptions()
        {
            Background = true
        }));

        var dateAndSpaceTypeDefinition = Builders<PageUpdateEvent>.IndexKeys.Descending(x => x.Updated).Ascending(x => x.SpaceType);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<PageUpdateEvent>(dateAndSpaceTypeDefinition, new CreateIndexOptions()
        {
            Background = true
        }));

        var dateAndSpaceTypeAndKeyDefinition = Builders<PageUpdateEvent>.IndexKeys.Descending(x => x.Updated).Ascending(x => x.SpaceType).Ascending(x => x.SpaceKey);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<PageUpdateEvent>(dateAndSpaceTypeAndKeyDefinition, new CreateIndexOptions()
        {
            Background = true
        }));

        var dateAndEmailKeyDefinition = Builders<PageUpdateEvent>.IndexKeys.Descending(x => x.Updated).Ascending(x => x.UpdatedBy.Email);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<PageUpdateEvent>(dateAndEmailKeyDefinition, new CreateIndexOptions()
        {
            Background = true
        }));

        var dateAndSpaceTypeAndEmailKeyDefinition = Builders<PageUpdateEvent>.IndexKeys.Descending(x => x.Updated).Ascending(x => x.SpaceType).Ascending(x => x.UpdatedBy.Email);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<PageUpdateEvent>(dateAndSpaceTypeAndEmailKeyDefinition, new CreateIndexOptions()
        {
            Background = true
        }));
    }
}