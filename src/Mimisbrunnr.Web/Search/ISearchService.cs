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
    /// <returns>Matching pages</returns>
    Task<IEnumerable<PageModel>> SearchPages(string text, UserInfo searchBy);

    /// <summary>
    /// Searches for users matching the specified text
    /// </summary>
    /// <param name="text">Search query</param>
    /// <param name="searchBy">User performing the search</param>
    /// <returns>Matching users</returns>
    Task<IEnumerable<UserModel>> SearchUsers(string text, UserInfo searchBy);
}