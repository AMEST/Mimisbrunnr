﻿using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Integration.Wiki;

public class SpaceCreateModel : IValidatableObject
{
    [Required]
    [MaxLength(64)]
    [RegularExpression("^[a-zA-Z0-9@\\.\\-_]*$", ErrorMessage = "Only Alphabets, Numbers, @, . , _ and - allowed.")]
    public string Key { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public SpaceTypeModel Type { get; set; }

    public string Description { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if(Key.Contains('@') && Type != SpaceTypeModel.Personal)
            return new []{new ValidationResult("Creating space with email as key allowed only for personal spaces.", new []{nameof(Key)})};

        return new []{ValidationResult.Success};
    }
}