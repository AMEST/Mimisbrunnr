using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Integration.Plugin;

/// <summary>A global Markdown/Mustache page template distributed with a plugin.</summary>
public class PluginPageTemplateModel
{
    /// <summary>Stable identifier unique within the plugin.</summary>
    [Required, MaxLength(255)]
    public string TemplateIdentifier { get; set; }

    /// <summary>Display name.</summary>
    [Required, MaxLength(255)]
    public string Name { get; set; }

    /// <summary>Optional description.</summary>
    [MaxLength(1024)]
    public string Description { get; set; }

    /// <summary>Markdown content with optional Mustache variables.</summary>
    [Required]
    public string Content { get; set; }
}
