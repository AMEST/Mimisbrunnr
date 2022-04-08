using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Web.Wiki;

public class SpaceCreateModel
{
    [Required]
    [MaxLength(12)]
    [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Only Alphabets and Numbers allowed.")]
    public string Key { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public SpaceTypeModel Type { get; set; }

    public string Description { get; set; }
}