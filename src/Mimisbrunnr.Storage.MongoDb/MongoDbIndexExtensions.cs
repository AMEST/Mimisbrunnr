using System.Linq;
using System.Text.RegularExpressions;
using MongoDB.Driver;

namespace Mimisbrunnr.Storage.MongoDb
{
    public static class MongoDbIndexExtensions
    {
        private static readonly Regex _conflictIndexNameSearch = new Regex("name: \\\"([^\\s\\\"]*)\\\"", RegexOptions.Compiled);
        private const int IndexOptionsConflict = 85;
        private const int IndexKeySpecsConflict = 86;

        public static async Task<string> TryCreateOneAsync<TDocument>(this IMongoIndexManager<TDocument> indexManager, CreateIndexModel<TDocument> model, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                return await indexManager.CreateOneAsync(model, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            catch (MongoCommandException e)
            {
                // If index exists with different options or name
                if (e.Code == IndexKeySpecsConflict || e.Code == IndexOptionsConflict)
                {
                    var conflictedIndexNameMatches = _conflictIndexNameSearch.Matches(e.Message);
                    var conflictedIndexNameMatch = conflictedIndexNameMatches.LastOrDefault();
                    var conflictedIndexName = conflictedIndexNameMatch.Groups.Count > 1 
                        ? conflictedIndexNameMatch.Groups[1].Value
                        : model.Options.Name;
                    await indexManager.DropOneAsync(conflictedIndexName, cancellationToken).ConfigureAwait(false);
                    return await indexManager.CreateOneAsync(model, cancellationToken: cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}