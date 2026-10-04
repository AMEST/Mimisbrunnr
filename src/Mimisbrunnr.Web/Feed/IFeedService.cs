using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Feed;

/// <summary>
/// Service for retrieving page update feeds
/// </summary>
public interface IFeedService
{
    /// <summary>
    /// Gets page updates, optionally filtered by the email of the user who updated them
    /// </summary>
    /// <param name="requestedBy">User requesting the feed</param>
    /// <param name="updatedByEmail">Optional email to filter updates by</param>
    /// <returns>Page update events</returns>
    Task<PageUpdateEventModel[]> GetPageUpdates(UserInfo requestedBy, string updatedByEmail = null);
}