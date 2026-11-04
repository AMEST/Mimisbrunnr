using Mimisbrunnr.Integration.User;

namespace Mimisbrunnr.Integration.PageTemplates;

/// <summary>
/// Page template model returned by the API
/// </summary>
public class PageTemplateModel
{
    /// <summary>
    /// Unique template identifier
    /// </summary>
    public string Id { get; set; }

    /// <summary>Identifier of the source plugin, or null for standalone templates.</summary>
    public string PluginIdentifier { get; set; }

    /// <summary>Display name of the source plugin.</summary>
    public string PluginName { get; set; }

    /// <summary>Whether the template can only be changed through its plugin package.</summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// Template name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Template description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Template content (Mustache/Markdown)
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// Template type: System, User, or Space
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Email of the template owner
    /// </summary>
    public string OwnerEmail { get; set; }

    /// <summary>
    /// Space ID (for Space-type templates)
    /// </summary>
    public string SpaceId { get; set; }

    /// <summary>
    /// Template creation date
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Template last update date
    /// </summary>
    public DateTime Updated { get; set; }

    /// <summary>
    /// User who created the template
    /// </summary>
    public UserModel CreatedBy { get; set; }

    /// <summary>
    /// User who last updated the template
    /// </summary>
    public UserModel UpdatedBy { get; set; }
}
