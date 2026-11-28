using Mimisbrunnr.Wiki.Contracts;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Wiki.Repositories;

/// <summary>
/// Page storage operations that cannot be expressed with <see cref="IRepository{TEntity}"/> without loosing efficiency
/// </summary>
public interface IPageRepository : IRepository<Page>
{
    /// <summary>
    /// Loads pages by identifiers in a single request
    /// </summary>
    Task<Page[]> GetByIds(string[] ids);

    /// <summary>
    /// Moves pages to another space without rewriting the whole documents
    /// </summary>
    Task MoveToSpace(string[] pageIds, string spaceId);

    /// <summary>
    /// Moves a single page by updating its space and parent in the same write
    /// </summary>
    /// <exception cref="PageConcurrencyException">The page was removed concurrently</exception>
    Task Move(string pageId, string spaceId, string parentId);

    /// <summary>
    /// Re-parents all direct children of the page
    /// </summary>
    Task ReparentChildren(string parentId, string newParentId);

    /// <summary>
    /// Replaces the page only when the stored version still equals <paramref name="expectedVersion"/>
    /// </summary>
    /// <exception cref="PageConcurrencyException">The page was modified concurrently</exception>
    Task ReplaceWithVersionCheck(Page page, long expectedVersion);
}
