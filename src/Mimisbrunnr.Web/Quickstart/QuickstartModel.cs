using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Web.Quickstart;

public class QuickstartModel
{
    [Required]
    [MaxLength(32)]
    public string Title { get; set; }

    [Required]
    public bool AllowAnonymous { get; set; }

    [Required]
    public bool SwaggerEnabled {get; set;}

    [Required]
    public bool AllowHtml { get; set; }

    public bool CustomHomepageEnabled { get; set; }

    public bool CustomHomepageKey { get; set; }
}