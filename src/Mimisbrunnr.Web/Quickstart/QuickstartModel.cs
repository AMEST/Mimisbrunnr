using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Web.Quickstart;

public class QuickstartModel
{
    [Required]
    [MaxLength(32)]
    public string Title { get; set; }

    [Required]
    public bool AllowAnonymous { get; set; }
}