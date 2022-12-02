using Mimisbrunnr.Integration.User;

namespace Mimisbrunnr.Integration.Wiki;

public class AttachmentModel
{
    public string Name { get; set; }

    public DateTime Created { get; set; }

    public UserModel CreatedBy { get; set; }
}
