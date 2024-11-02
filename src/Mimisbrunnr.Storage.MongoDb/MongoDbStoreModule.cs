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
using Mimisbrunnr.Wiki.Services;
using Mimisbrunnr.Storage.MongoDb.Migrations;
using Mimisbrunnr.Favorites.Contracts;
using MongoDB.Bson.Serialization;
using Mimisbrunnr.Favorites.Services;

namespace Mimisbrunnr.Storage.MongoDb;

public class MongoDbStoreModule : RunnableModule
{
    public override Type[] DependsModules => [ 
        typeof(PersonalSpaceAvatarMigrationModule),
        typeof(SpacesFlatPermissionMigrationModule)
     ];
     
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
                .AddEntity<HistoricalPage, HistoricalPageMap>()
                .AddEntity<Draft, DraftMap>()
                .AddEntity<PageUpdateEvent, PageUpdateEventMap>()
                .AddEntity<Attachment, AttachmentMap>()
                .AddEntity<UserToken, UserTokenMap>()
                .AddEntity<Favorite, FavoriteMap>()
                .AddEntity<Comment, CommentMap>()
        );
        BsonClassMap.RegisterClassMap(new FavoriteUserMap());
        BsonClassMap.RegisterClassMap(new FavoriteSpaceMap());
        BsonClassMap.RegisterClassMap(new FavoritePageMap());

        services
            .AddSingleton<IPageSearcher, PageSearcher>()
            .AddSingleton<IFavoriteStore, FavoriteStore>();
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
            await CreatePageHistoryIndexes(baseMongoContext);
            await CreatePageDraftIndexes(baseMongoContext);
            await CreatePageUpdatesIndexes(baseMongoContext);
            await CreateAttachmentIndexes(baseMongoContext);
            await CreateUserTokenIndexes(baseMongoContext);
            await CreateFavoriteIndexes(baseMongoContext);
            await CreateCommentIndexes(baseMongoContext);
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
        var ownersKeyDefinition = Builders<Group>.IndexKeys.Ascending(x => x.OwnerEmails);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<Group>(ownersKeyDefinition, new CreateIndexOptions()
        {
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

        var visibleSpaces = Builders<Space>.IndexKeys.Ascending(x => x.PermissionsFlat).Ascending(x => x.Type);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<Space>(visibleSpaces, new CreateIndexOptions()
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


    private async Task CreatePageHistoryIndexes(BaseMongoDbContext mongoContext)
    {
        var collection = mongoContext.GetCollection<HistoricalPage>();
        var pageIdDefinition = Builders<HistoricalPage>.IndexKeys.Ascending(x => x.PageId);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<HistoricalPage>(pageIdDefinition, new CreateIndexOptions()
        {
            Background = true
        }));
        var pageIdAndVersionDefinition = Builders<HistoricalPage>.IndexKeys.Ascending(x => x.PageId).Ascending(x => x.Version);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<HistoricalPage>(pageIdAndVersionDefinition, new CreateIndexOptions()
        {
            Background = true
        }));
    }


    private static async Task CreatePageDraftIndexes(IMongoDbContext mongoContext)
    {
        var collection = mongoContext.GetCollection<Draft>();
        var pageDraftDefinition = Builders<Draft>.IndexKeys.Ascending(x => x.OriginalPageId);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<Draft>(pageDraftDefinition, new CreateIndexOptions()
        {
            Background = true,
            Unique = true
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

    private static async Task CreateAttachmentIndexes(IMongoDbContext mongoContext)
    {
        var collection = mongoContext.GetCollection<Attachment>();
        var nameKeyDefinition = Builders<Attachment>.IndexKeys.Ascending(x => x.Name);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<Attachment>(nameKeyDefinition, new CreateIndexOptions()
        {
            Background = true
        }));
        var pageIdDefinition = Builders<Attachment>.IndexKeys.Ascending(x => x.PageId);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<Attachment>(pageIdDefinition, new CreateIndexOptions()
        {
            Background = true
        }));
        var nameAndPageIdDefinition = Builders<Attachment>.IndexKeys.Ascending(x => x.PageId).Ascending(x => x.Name);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<Attachment>(nameAndPageIdDefinition, new CreateIndexOptions()
        {
            Background = true,
            Unique = true
        }));
    }


    private static async Task CreateUserTokenIndexes(IMongoDbContext mongoContext)
    {
        var collection = mongoContext.GetCollection<UserToken>();
        var userTokenDefinition = Builders<UserToken>.IndexKeys.Ascending(x => x.UserId);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<UserToken>(userTokenDefinition, new CreateIndexOptions()
        {
            Background = true
        }));
        var revokeTokenDefinition = Builders<UserToken>.IndexKeys.Ascending(x => x.Id).Ascending(x => x.UserId);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<UserToken>(revokeTokenDefinition, new CreateIndexOptions()
        {
            Background = true
        }));
        var ensureRevokedTokenDefinition = Builders<UserToken>.IndexKeys.Ascending(x => x.Id).Ascending(x => x.Revoked);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<UserToken>(ensureRevokedTokenDefinition, new CreateIndexOptions()
        {
            Background = true
        }));
    }

    private static async Task CreateFavoriteIndexes(BaseMongoDbContext mongoContext)
    {
        var collection = mongoContext.GetCollection<Favorite>();
        var userIdKeyDefinition = Builders<Favorite>.IndexKeys
            .Ascending("_t")
            .Ascending(x => x.OwnerEmail);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<Favorite>(userIdKeyDefinition, new CreateIndexOptions()
        {
            Background = true
        }));
        var favoriteUserIndex = Builders<FavoriteUser>.IndexKeys
            .Ascending("_t")
            .Ascending(x => x.OwnerEmail)
            .Ascending(x => x.UserEmail);
        await collection.OfType<FavoriteUser>().Indexes.CreateOneAsync(new CreateIndexModel<FavoriteUser>(favoriteUserIndex, new CreateIndexOptions()
        {
            Background = true
        }));
        var favoriteSpaceIndex = Builders<FavoriteSpace>.IndexKeys
            .Ascending("_t")
            .Ascending(x => x.OwnerEmail)
            .Ascending(x => x.SpaceKey);
        await collection.OfType<FavoriteSpace>().Indexes.CreateOneAsync(new CreateIndexModel<FavoriteSpace>(favoriteSpaceIndex, new CreateIndexOptions()
        {
            Background = true
        }));
        var favoritePageIndex = Builders<FavoritePage>.IndexKeys
            .Ascending("_t")
            .Ascending(x => x.OwnerEmail)
            .Ascending(x => x.PageId);
        await collection.OfType<FavoritePage>().Indexes.CreateOneAsync(new CreateIndexModel<FavoritePage>(favoritePageIndex, new CreateIndexOptions()
        {
            Background = true
        }));
    }

    private static async Task CreateCommentIndexes(BaseMongoDbContext mongoContext)
    {
        var collection = mongoContext.GetCollection<Comment>();
        var pageIdKeyDefinition = Builders<Comment>.IndexKeys.Ascending(x => x.PageId);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<Comment>(pageIdKeyDefinition, new CreateIndexOptions()
        {
            Background = true
        }));
    }
}