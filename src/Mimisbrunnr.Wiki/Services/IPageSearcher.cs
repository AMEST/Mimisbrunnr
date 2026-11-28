using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Wiki.Services;

public interface IPageSearcher
{
    /// <summary>
    /// Searches pages by full text across all spaces.
    /// </summary>
    Task<Page[]> Search(string text);

    /// <summary>
    /// Searches pages by full text in relevance order within <paramref name="spaceIds"/>.
    /// </summary>
    /// <param name="text">Search query</param>
    /// <param name="spaceIds">Spaces visible to the caller. <c>null</c> means no space filtering.</param>
    /// <param name="limit">Maximum number of pages to return</param>
    /// <param name="offset">Number of matching pages to skip</param>
    Task<Page[]> Search(string text, string[] spaceIds, int limit = 100, int offset = 0);
}
