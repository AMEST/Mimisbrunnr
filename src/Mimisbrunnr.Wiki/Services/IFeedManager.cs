using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Wiki.Services;

public interface IFeedManager
{
     Task<PageUpdateEvent[]> GetAllPageUpdates();

     Task<PageUpdateEvent[]> GetPageUpdates(UserInfo requestedBy, IEnumerable<Space> userSpaces = null, UserInfo updatedBy = null);

     Task AddPageUpdate(Space space, Page page, UserInfo updatedBy);
}