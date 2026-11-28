using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Mimisbrunnr.PageTemplates.Contracts;
using Mimisbrunnr.Storage.MongoDb.Mappings;
using Mimisbrunnr.Wiki.Contracts;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Web.Tests.Infrastructure;

[CollectionDefinition(nameof(MongoDbTestDatabase))]
public class MongoDbTestCollection : ICollectionFixture<MongoDbTestDatabase>;

public sealed class MongoDbTestDatabase : IAsyncLifetime
{
    internal const string ConnectionStringVariable = "MIMISBRUNNR_TEST_MONGODB";
    private ServiceProvider _services;

    public IMongoDatabase Database { get; private set; }
    public ConcurrentQueue<(string Collection, string Pipeline)> Aggregations { get; } = new();

    public Task InitializeAsync()
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable);
        if (string.IsNullOrWhiteSpace(connectionString))
            return Task.CompletedTask;

        var url = new MongoUrlBuilder(connectionString)
        {
            DatabaseName = $"mimisbrunnr_test_{Guid.NewGuid():N}",
            ServerSelectionTimeout = TimeSpan.FromSeconds(5)
        };
        var settings = MongoClientSettings.FromUrl(url.ToMongoUrl());
        settings.ClusterConfigurator = cluster => cluster.Subscribe<CommandStartedEvent>(command =>
        {
            if (command.CommandName == "aggregate")
                Aggregations.Enqueue((command.Command["aggregate"].AsString, command.Command["pipeline"].ToJson()));
        });
        Database = new MongoClient(settings).GetDatabase(url.DatabaseName);

        var services = new ServiceCollection();
        services.AddMongoDbContext(builder => builder
            .UseConnectionString(url.ToString())
            .AddEntity<Page, PageMap>()
            .AddEntity<Space, SpaceMap>()
            .AddEntity<PageTemplate, PageTemplateMap>());
        _services = services.BuildServiceProvider();
        return Task.CompletedTask;
    }

    public T CreateRepository<T>() => ActivatorUtilities.CreateInstance<T>(_services);

    public async Task DisposeAsync()
    {
        if (_services is null)
            return;

        try
        {
            await Database.Client.DropDatabaseAsync(Database.DatabaseNamespace.DatabaseName);
        }
        finally
        {
            await _services.DisposeAsync();
        }
    }
}

internal sealed class MongoDbFactAttribute : FactAttribute
{
    public MongoDbFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(MongoDbTestDatabase.ConnectionStringVariable)))
            Skip = $"Set {MongoDbTestDatabase.ConnectionStringVariable} to run MongoDB integration tests.";
    }
}
