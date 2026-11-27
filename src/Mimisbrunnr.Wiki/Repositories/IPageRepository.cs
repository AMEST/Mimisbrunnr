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
    /// Changes the parent of a single page
    /// </summary>
    Task Reparent(string pageId, string newParentId);

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
