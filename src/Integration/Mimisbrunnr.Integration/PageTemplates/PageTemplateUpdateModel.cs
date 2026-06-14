using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Integration.PageTemplates;

/// <summary>
/// Request model for updating an existing page template
/// </summary>
public class PageTemplateUpdateModel
{
    /// <summary>
    /// Template name
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// Template description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Template content (Mustache/Markdown)
    /// </summary>
    [Required]
    public string Content { get; set; }
}
