using Mimisbrunnr.Integration.User;

namespace Mimisbrunnr.Web.Feed;

/// <summary>
/// Model representing a page update event
/// </summary>
public class PageUpdateEventModel
{
    /// <summary>
    /// Key of the space containing the page
    /// </summary>
    public string SpaceKey { get; set; }

    /// <summary>
    /// Identifier of the updated page
    /// </summary>
    public string PageId { get; set; }
    
    /// <summary>
    /// Title of the updated page
    /// </summary>
    public string PageTitle { get; set; }

    /// <summary>
    /// Date and time when the page was updated
    /// </summary>
    public DateTime Updated { get; set; }

    /// <summary>
    /// User who updated the page
    /// </summary>
    public UserModel UpdatedBy { get; set; }
}