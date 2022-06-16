using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Web.Administration;

public class ApplicationConfigurationModel
{
    [Required]
    public string Title { get; set; }

    public bool AllowAnonymous { get; set; }

    public bool SwaggerEnabled { get; set; }

    public bool AllowHtml { get; set; }

    public bool CustomHomepageEnabled { get; set; }

    public bool CustomHomepageKey { get; set; }
}