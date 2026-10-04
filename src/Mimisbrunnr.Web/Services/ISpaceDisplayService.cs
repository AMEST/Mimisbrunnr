using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Services;

/// <summary>
/// Service for retrieving spaces visible to a user
/// </summary>
public interface ISpaceDisplayService
{
    /// <summary>
    /// Finds spaces visible to the specified user
    /// </summary>
    /// <param name="userInfo">User whose visible spaces are requested</param>
    /// <param name="take">Maximum number of spaces to return</param>
    /// <param name="skip">Number of spaces to skip</param>
    /// <returns>The spaces visible to the user</returns>
    Task<IEnumerable<Space>> FindUserVisibleSpaces(UserInfo userInfo, int? take = null, int? skip = null);
}