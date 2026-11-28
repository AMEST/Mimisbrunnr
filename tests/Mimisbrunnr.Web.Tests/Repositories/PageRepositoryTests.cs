using FluentAssertions;
using Mimisbrunnr.Storage.MongoDb.Repositories;
using Mimisbrunnr.Web.Tests.Infrastructure;
using Mimisbrunnr.Wiki.Contracts;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimisbrunnr.Web.Tests.Repositories;

[Collection(nameof(MongoDbTestDatabase))]
[Trait("Category", "Integration")]
public class PageRepositoryTests(MongoDbTestDatabase database)
{
    [MongoDbFact]
    public async Task Should_MovePageWithoutChangingContent_WhenSpaceAndParentChange()
    {
        var documents = database.Database.GetCollection<Page>("Pages");
        var source = new Page
        {
            Id = "move-page", SpaceId = "old-space", ParentId = "old-parent",
            Name = "Page title", Content = "Page text", Version = 2
        };
        await documents.InsertOneAsync(source);
        var repository = database.CreateRepository<PageRepository>();

        await repository.Move(source.Id, "new-space", "new-parent");

        var saved = (await repository.GetByIds([source.Id])).Should().ContainSingle().Which;
        saved.SpaceId.Should().Be("new-space");
        saved.ParentId.Should().Be("new-parent");
        saved.Content.Should().Be(source.Content);
        saved.Version.Should().Be(source.Version);
    }

    [MongoDbFact]
    public async Task ShouldThrow_PageConcurrencyException_WhenMovingMissingPage()
    {
        var repository = database.CreateRepository<PageRepository>();

        await repository.Invoking(x => x.Move("removed-page", "new-space", "new-parent"))
            .Should().ThrowAsync<PageConcurrencyException>();
    }

    [MongoDbFact]
    public async Task Should_ReplacePage_WhenStoredVersionIsMissingAndExpectedVersionIsZero()
    {
        var documents = database.Database.GetCollection<BsonDocument>("Pages");
        await documents.InsertOneAsync(new BsonDocument { { "_id", "legacy-page" }, { "Name", "Before" } });
        var repository = database.CreateRepository<PageRepository>();
        var page = (await repository.GetByIds(["legacy-page"])).Should().ContainSingle().Which;
        page.Version.Should().Be(0);
        page.Name = "After";
        page.Version = 1;

        await repository.ReplaceWithVersionCheck(page, 0);

        var saved = (await repository.GetByIds([page.Id])).Should().ContainSingle().Which;
        saved.Version.Should().Be(1);
        saved.Name.Should().Be("After");
        await repository.Invoking(x => x.ReplaceWithVersionCheck(page, 0))
            .Should().ThrowAsync<PageConcurrencyException>();
    }

    [MongoDbFact]
    public async Task ShouldThrow_PageConcurrencyException_WhenStoredVersionIsMissingAndExpectedVersionIsNonzero()
    {
        var documents = database.Database.GetCollection<BsonDocument>("Pages");
        await documents.InsertOneAsync(new BsonDocument("_id", "unversioned-page"));
        var repository = database.CreateRepository<PageRepository>();
        var page = new Page { Id = "unversioned-page", Version = 2 };

        await repository.Invoking(x => x.ReplaceWithVersionCheck(page, 1))
            .Should().ThrowAsync<PageConcurrencyException>();
    }
}
