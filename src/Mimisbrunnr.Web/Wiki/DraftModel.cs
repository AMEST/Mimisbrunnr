using Mimisbrunnr.Integration.User;

namespace Mimisbrunnr.Web.Wiki;

/// <summary>
/// Represents a page draft
/// </summary>
public class DraftModel
{
    /// <summary>
    /// Draft name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Draft content
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// Date and time when the draft was last updated
    /// </summary>
    public DateTime Updated { get; set; }
    
    /// <summary>
    /// User who last updated the draft
    /// </summary>
    public UserModel UpdatedBy { get; set; }
    
}