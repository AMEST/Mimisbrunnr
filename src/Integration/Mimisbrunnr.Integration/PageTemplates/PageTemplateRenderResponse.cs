namespace Mimisbrunnr.Integration.PageTemplates;

/// <summary>
/// Response model containing rendered template content
/// </summary>
public class PageTemplateRenderResponse
{
    /// <summary>
    /// Rendered template content (HTML/Markdown)
    /// </summary>
    public string Content { get; set; }
}
