using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Web.Wiki;

public class PageUpdateModel
{
    [Required]
    public string Name { get; set; }

    public string Content { get; set; }
}