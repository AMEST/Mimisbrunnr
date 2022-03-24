using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Web.Wiki;

public class SpaceCreateModel
{
    [Required]
    public string Key { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    [Required]
    public SpaceTypeModel Type { get; set; }
    
    public string Description { get; set; }
}