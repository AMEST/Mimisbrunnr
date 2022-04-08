using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Web.Wiki;

public class SpaceCreateModel
{
    [Required]
    [MaxLength(24)]
    [RegularExpression("^[a-zA-Z0-9@\\.\\-_]*$", ErrorMessage = "Only Alphabets, Numbers, @, . , _ and - allowed.")]
    public string Key { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public SpaceTypeModel Type { get; set; }

    public string Description { get; set; }
}