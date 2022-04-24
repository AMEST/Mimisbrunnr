using Mimisbrunnr.Web.User;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Feed;

public class PageUpdateEventModel
{
    public string SpaceKey { get; set; }

    public string PageTitle { get; set; }

    public DateTime Updated { get; set; }

    public UserModel UpdatedBy { get; set; }
}