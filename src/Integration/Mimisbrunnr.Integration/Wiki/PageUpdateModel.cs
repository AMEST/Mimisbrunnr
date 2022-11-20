using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Integration.Wiki;

public class PageUpdateModel
{
    [Required]
    public string Name { get; set; }

    public string Content { get; set; }
}