using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Web.Wiki;

public class DraftUpdateModel
{

    [Required]
    public string Name { get; set; }

    public string Content { get; set; }
}