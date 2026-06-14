using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Integration.PageTemplates;

/// <summary>
/// Request model for creating a new page template
/// </summary>
public class PageTemplateCreateModel
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

    /// <summary>
    /// Template type: System, User, or Space
    /// </summary>
    [Required]
    public string Type { get; set; }

    /// <summary>
    /// Space key (required for Space-type templates)
    /// </summary>
    public string SpaceKey { get; set; }
}
