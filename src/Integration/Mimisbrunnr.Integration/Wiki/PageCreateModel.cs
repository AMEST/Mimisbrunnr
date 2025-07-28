using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Integration.Wiki;

/// <summary>
/// Model for creating a new wiki page
/// </summary>
public class PageCreateModel
{
    /// <summary>
    /// Key of the space where the page will be created (required)
    /// </summary>
    [Required]
    public string SpaceKey { get; set; }

    /// <summary>
    /// ID of the parent page (required)
    /// </summary>
    [Required]
    public string ParentPageId { get; set; }

    /// <summary>
    /// Name of the page (required)
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// Content of the page
    /// </summary>
    public string Content { get; set; }
}
