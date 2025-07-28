using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Integration.Wiki;

/// <summary>
/// Model for creating a new wiki space
/// </summary>
public class SpaceCreateModel : IValidatableObject
{
    /// <summary>
    /// Unique space key (required, max length: 64, allowed characters: alphanumeric, @, ., -, _)
    /// </summary>
    [Required]
    [MaxLength(64)]
    [RegularExpression("^[a-zA-Z0-9@\\.\\-_]*$", ErrorMessage = "Only Alphabets, Numbers, @, . , _ and - allowed.")]
    public string Key { get; set; }

    /// <summary>
    /// Space name (required)
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// Space type (required)
    /// </summary>
    [Required]
    public SpaceTypeModel Type { get; set; }

    /// <summary>
    /// Space description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Validates space creation model
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if(Key.Contains('@') && Type != SpaceTypeModel.Personal)
            return new []{new ValidationResult("Creating space with email as key allowed only for personal spaces.", new []{nameof(Key)})};

        return new []{ValidationResult.Success};
    }
}
