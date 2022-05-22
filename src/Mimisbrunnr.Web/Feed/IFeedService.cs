using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Feed;

public interface IFeedService
{
    Task<PageUpdateEventModel[]> GetPageUpdates(UserInfo requestedBy, string updatedByEmail = null);
}