using Mimisbrunnr.Integration.User;

namespace Mimisbrunnr.Web.Wiki;

public class DraftModel
{
    public string Name { get; set; }

    public string Content { get; set; }

    public DateTime Updated { get; set; }
    
    public UserModel UpdatedBy { get; set; }
    
}