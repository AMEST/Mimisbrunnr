using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Integration.Wiki;

/// <summary>
/// Model for updating a wiki page
/// </summary>
public class PageUpdateModel
{
    /// <summary>
    /// New page name (required)
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// New page content
    /// </summary>
    public string Content { get; set; }
}
