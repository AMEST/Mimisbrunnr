using Mimisbrunnr.Web.User;

namespace Mimisbrunnr.Web.Wiki;

public class AttachmentModel
{
    public string Name { get; set; }

    public DateTime Created { get; set; }

    public UserModel CreatedBy { get; set; }
}
