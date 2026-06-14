using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Integration.PageTemplates;

/// <summary>
/// Request model for rendering a page template
/// </summary>
public class PageTemplateRenderRequest
{
    /// <summary>
    /// Template ID to render
    /// </summary>
    [Required]
    public string TemplateId { get; set; }

    /// <summary>
    /// Space key used for context variables during rendering
    /// </summary>
    [Required]
    public string SpaceKey { get; set; }
}
