using System.Text.RegularExpressions;
using Mimisbrunnr.Wiki.Contracts;
using Mimisbrunnr.Wiki.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;
using Skidbladnir.Repository.MongoDB;

namespace Mimisbrunnr.Storage.MongoDb.Repositories;

public class SpaceRepository : MongoRepository<Space, BaseMongoDbContext>, ISpaceRepository
{
    public SpaceRepository(BaseMongoDbContext context, MongoDbContextConfiguration<BaseMongoDbContext> configuration)
        : base(context, configuration)
    {
    }

    public async Task<Space[]> RemoveGroupPermissions(string groupName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupName);

        var groupPermission = Builders<Permission>.Filter.Regex(
            permission => permission.Group.Name,
            new BsonRegularExpression($@"\A{Regex.Escape(groupName)}\z", "i"));
        var filter = Builders<Space>.Filter.ElemMatch(x => x.Permissions, groupPermission);
        var affected = await Entities.Find(filter).ToListAsync();
        if (affected.Count == 0)
            return [];

        // The flattened permission list is used by visibility queries, so it must be updated in the same request.
        var update = Builders<Space>.Update
            .PullFilter(x => x.Permissions, groupPermission)
            .Pull(x => x.PermissionsFlat, $"group_{groupName.ToLower()}");

        await Entities.UpdateManyAsync(filter, update);
        return [.. affected];
    }
}
