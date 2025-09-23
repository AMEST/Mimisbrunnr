using Mimisbrunnr.Integration.User;

namespace Mimisbrunnr.Integration.Wiki;

/// <summary>
/// Wiki page model
/// </summary>
public class PageModel
{
    /// <summary>
    /// Unique page identifier
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Current page version
    /// </summary>
    public long Version { get; set; }

    /// <summary>
    /// Key of the space this page belongs to
    /// </summary>
    public string SpaceKey { get; set; }

    /// <summary>
    /// Page name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Page content
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// Page creation date
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Last page update date
    /// </summary>
    public DateTime Updated { get; set; }

    /// <summary>
    /// User who created the page
    /// </summary>
    public UserModel CreatedBy { get; set; }

    /// <summary>
    /// User who last updated the page
    /// </summary>
    public UserModel UpdatedBy { get; set; }
}
