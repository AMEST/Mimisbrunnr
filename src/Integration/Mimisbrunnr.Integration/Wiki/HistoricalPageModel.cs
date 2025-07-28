using Mimisbrunnr.Integration.User;

namespace Mimisbrunnr.Integration.Wiki;

/// <summary>
/// Historical page version model
/// </summary>
public class HistoricalPageModel
{
    /// <summary>
    /// Unique identifier of the historical version
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Identifier of the page this version belongs to
    /// </summary>
    public string PageId { get; set; }

    /// <summary>
    /// Version number
    /// </summary>
    public long Version { get; set; }

    /// <summary>
    /// Page name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Page content
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// Creation date of this version
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Last update date of this version
    /// </summary>
    public DateTime Updated { get; set; }

    /// <summary>
    /// User who last updated this version
    /// </summary>
    public UserModel UpdatedBy { get; set; }
}
