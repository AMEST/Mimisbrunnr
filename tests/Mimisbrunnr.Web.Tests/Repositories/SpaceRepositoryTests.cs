using FluentAssertions;
using Mimisbrunnr.Storage.MongoDb.Repositories;
using Mimisbrunnr.Web.Tests.Infrastructure;
using Mimisbrunnr.Wiki.Contracts;
using MongoDB.Driver;

namespace Mimisbrunnr.Web.Tests.Repositories;

[Collection(nameof(MongoDbTestDatabase))]
[Trait("Category", "Integration")]
public class SpaceRepositoryTests(MongoDbTestDatabase database)
{
    [MongoDbFact]
    public async Task Should_RemoveGroupGrants_WhenPermissionNamesHaveDifferentCasing()
    {
        var spaces = database.Database.GetCollection<Space>("Spaces");
        var affected = new Space
        {
            Id = "affected-space", Key = "AFFECTED",
            Permissions =
            [
                new Permission { Group = new GroupInfo { Name = "oPs.+" }, CanView = true },
                new Permission { Group = new GroupInfo { Name = "OPS.+" }, CanEdit = true },
                new Permission { Group = new GroupInfo { Name = "Ops-team" }, CanView = true },
                new Permission { User = new UserInfo { Email = "user@example.test" }, CanView = true }
            ]
        };
        var unaffected = new Space
        {
            Id = "other-space", Key = "OTHER",
            Permissions = [new Permission { Group = new GroupInfo { Name = "PrefixOps.+" }, CanView = true }]
        };
        affected.UpdatePermissions();
        unaffected.UpdatePermissions();
        await spaces.InsertManyAsync([affected, unaffected]);
        var repository = database.CreateRepository<SpaceRepository>();

        var result = await repository.RemoveGroupPermissions("Ops.+");

        result.Should().ContainSingle().Which.Id.Should().Be(affected.Id);
        var changed = await spaces.Find(x => x.Id == affected.Id).SingleAsync();
        changed.Permissions.Should().BeEquivalentTo(affected.Permissions.Skip(2));
        changed.PermissionsFlat.Should().BeEquivalentTo("group_ops-team", "user_user@example.test");
        var other = await spaces.Find(x => x.Id == unaffected.Id).SingleAsync();
        other.Permissions.Should().BeEquivalentTo(unaffected.Permissions);
        other.PermissionsFlat.Should().BeEquivalentTo(unaffected.PermissionsFlat);
    }
}
