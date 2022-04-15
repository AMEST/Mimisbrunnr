using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Web.Wiki;

public class PageCreateModel
{
    [Required]
    public string SpaceKey { get; set; }
    
    [Required]
    public string ParentPageId { get; set; }

    [Required]
    public string Name { get; set; }

    public string Content { get; set; }
}