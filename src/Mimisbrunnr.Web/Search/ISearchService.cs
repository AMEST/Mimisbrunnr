using Mimisbrunnr.Integration.User;
using Mimisbrunnr.Integration.Wiki;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Search;

/// <summary>
/// Service for searching across wiki entities
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// Searches for spaces matching the specified text
    /// </summary>
    /// <param name="text">Search query</param>
    /// <param name="searchBy">User performing the search</param>
    /// <returns>Matching spaces</returns>
    Task<IEnumerable<SpaceModel>> SearchSpaces(string text, UserInfo searchBy);

    /// <summary>
    /// Searches for pages matching the specified text
    /// </summary>
    /// <param name="text">Search query</param>
    /// <param name="searchBy">User performing the search</param>
    /// <returns>Matching pages in relevance order</returns>
    Task<IEnumerable<PageModel>> SearchPages(string text, UserInfo searchBy);

    /// <summary>
    /// Searches for pages matching the specified text and reports the offset of the next results
    /// </summary>
    /// <param name="text">Search query</param>
    /// <param name="searchBy">User performing the search</param>
    /// <param name="limit">Maximum number of matching pages to return</param>
    /// <param name="offset">Number of matching pages to skip</param>
    /// <returns>Matching pages in relevance order and an offset when more matches exist</returns>
    Task<PageSearchResult> SearchPages(string text, UserInfo searchBy, int limit, int offset);

    /// <summary>
    /// Searches for users matching the specified text
    /// </summary>
    /// <param name="text">Search query</param>
    /// <param name="searchBy">User performing the search</param>
    /// <returns>Matching users</returns>
    Task<IEnumerable<UserModel>> SearchUsers(string text, UserInfo searchBy);
}