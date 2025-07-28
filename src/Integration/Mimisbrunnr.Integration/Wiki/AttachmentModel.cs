using Mimisbrunnr.Integration.User;

namespace Mimisbrunnr.Integration.Wiki;

/// <summary>
/// Attachment model (file) in Wiki
/// </summary>
public class AttachmentModel
{
    /// <summary>
    /// Name of file
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Creation date
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Created by user (who upload attachment)
    /// </summary>
    public UserModel CreatedBy { get; set; }
}
