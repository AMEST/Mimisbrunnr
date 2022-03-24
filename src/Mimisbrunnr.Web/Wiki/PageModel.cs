using Mimisbrunnr.Web.Authentication.Account;

namespace Mimisbrunnr.Web.Wiki;

public class PageModel
{
    public string Id { get; set; }

    public string SpaceKey { get; set; }

    public string Name { get; set; }

    public string Content { get; set; }

    public DateTime Created { get; set; }

    public DateTime Updated { get; set; }
    
    public UserModel CreatedBy { get; set; }
    
    public UserModel UpdatedBy { get; set; }
}