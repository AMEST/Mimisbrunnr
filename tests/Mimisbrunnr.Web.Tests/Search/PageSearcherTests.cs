using FluentAssertions;
using Mimisbrunnr.Storage.MongoDb;
using Mimisbrunnr.Web.Tests.Infrastructure;
using Mimisbrunnr.Wiki.Contracts;
using MongoDB.Driver;

namespace Mimisbrunnr.Web.Tests.Search;

[Collection(nameof(MongoDbTestDatabase))]
[Trait("Category", "Integration")]
public class PageSearcherTests(MongoDbTestDatabase database)
{
    [MongoDbFact]
    public async Task Should_RankAndPageOnlyVisibleMatches_WhenSearchingAcrossSpaces()
    {
        const string visibleSpace = "search-visible";
        const string hiddenSpace = "search-hidden";
        var pages = database.Database.GetCollection<Page>("Pages");
        var textIndex = Builders<Page>.IndexKeys.Text(x => x.Name).Text(x => x.Content);
        await pages.Indexes.CreateOneAsync(new CreateIndexModel<Page>(textIndex));

        var documents = new List<Page>
        {
            new() { Id = "title-b", SpaceId = visibleSpace, Name = "handbook" },
            new() { Id = "title-a", SpaceId = visibleSpace, Name = "handbook" }
        };
        documents.AddRange(Enumerable.Range(0, 105).Select(i => new Page
        {
            Id = $"body-{i:D3}", SpaceId = visibleSpace, Name = "Notes",
            Content = "Read the handbook for more information about our operations"
        }));
        documents.AddRange(Enumerable.Range(0, 110).Select(i => new Page
        {
            Id = $"hidden-{i:D3}", SpaceId = hiddenSpace, Name = "handbook"
        }));
        await pages.InsertManyAsync(documents);
        var searcher = database.CreateRepository<PageSearcher>();

        var first = await searcher.Search("handbook", [visibleSpace], limit: 20);
        var second = await searcher.Search("handbook", [visibleSpace], limit: 20, offset: 20);
        var last = await searcher.Search("handbook", [visibleSpace], limit: 20, offset: 100);

        first.Select(x => x.Id).Should().Equal(new[] { "title-a", "title-b" }
            .Concat(Enumerable.Range(0, 18).Select(i => $"body-{i:D3}")));
        second.Select(x => x.Id).Should().Equal(Enumerable.Range(18, 20).Select(i => $"body-{i:D3}"));
        last.Select(x => x.Id).Should().Equal(Enumerable.Range(98, 7).Select(i => $"body-{i:D3}"));
    }
}
