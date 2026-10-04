using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Web.Wiki;

/// <summary>
/// Represents the data used to update a page draft
/// </summary>
public class DraftUpdateModel
{

    /// <summary>
    /// Draft name
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// Draft content
    /// </summary>
    public string Content { get; set; }
}