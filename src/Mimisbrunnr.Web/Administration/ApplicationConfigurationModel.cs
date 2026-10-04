using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Web.Administration;

/// <summary>
/// Model representing the application configuration
/// </summary>
public class ApplicationConfigurationModel
{
    /// <summary>
    /// Title of the application
    /// </summary>
    [Required]
    public string Title { get; set; }

    /// <summary>
    /// Whether anonymous access is allowed
    /// </summary>
    public bool AllowAnonymous { get; set; }

    /// <summary>
    /// Whether users are automatically created on first login
    /// </summary>
    public bool UserAutoCreation { get; set; }

    /// <summary>
    /// Whether the Swagger API documentation is enabled
    /// </summary>
    public bool SwaggerEnabled { get; set; }

    /// <summary>
    /// Whether raw HTML is allowed in page content
    /// </summary>
    public bool AllowHtml { get; set; }

    /// <summary>
    /// Custom CSS applied to the application
    /// </summary>
    public string CustomCss { get; set; }

    /// <summary>
    /// Whether a custom homepage is enabled
    /// </summary>
    public bool CustomHomepageEnabled { get; set; }

    /// <summary>
    /// Space key whose home page is displayed as the custom homepage
    /// </summary>
    public string CustomHomepageSpaceKey { get; set; }
}