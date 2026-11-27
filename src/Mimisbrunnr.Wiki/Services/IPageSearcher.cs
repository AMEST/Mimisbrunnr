using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Wiki.Services;

public interface IPageSearcher
{
    /// <summary>
    /// Searches pages by full text across all spaces.
    /// </summary>
    Task<Page[]> Search(string text);

    /// <summary>
    /// Searches pages by full text and returns only pages that belong to <paramref name="spaceIds"/>.
    /// </summary>
    /// <param name="text">Search query</param>
    /// <param name="spaceIds">Spaces visible to the caller. <c>null</c> means no space filtering.</param>
    /// <param name="limit">Maximum number of pages to return</param>
    Task<Page[]> Search(string text, string[] spaceIds, int limit = 100);
}
