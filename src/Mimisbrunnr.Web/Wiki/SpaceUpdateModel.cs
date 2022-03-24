using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Web.Wiki;

public class SpaceUpdateModel
{
    [Required]
    public string Name { get; set; }
    
    [Required]
    public string Description { get; set; }
}