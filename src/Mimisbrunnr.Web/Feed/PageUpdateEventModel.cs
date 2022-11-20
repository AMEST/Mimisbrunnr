using Mimisbrunnr.Integration.User;

namespace Mimisbrunnr.Web.Feed;

public class PageUpdateEventModel
{
    public string SpaceKey { get; set; }

    public string PageId { get; set; }
    
    public string PageTitle { get; set; }

    public DateTime Updated { get; set; }

    public UserModel UpdatedBy { get; set; }
}