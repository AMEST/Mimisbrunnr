using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Integration.Wiki;

public class SpaceUpdateModel : IValidatableObject
{
    [Required]
    public string Name { get; set; }

    [Required]
    public string Description { get; set; }

    public bool? Public { get; set; }

    public string AvatarUrl { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(AvatarUrl) || AvatarUrl.StartsWith("/api/attachment"))
            return new[] { ValidationResult.Success };

        return new[] { new ValidationResult("Avatar url only can be relative path.") };
    }
}