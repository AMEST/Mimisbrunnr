using System.ComponentModel.DataAnnotations;
using Mimisbrunnr.Web.Group;
using Mimisbrunnr.Web.User;

namespace Mimisbrunnr.Web.Wiki;

public class SpacePermissionModel : IValidatableObject
{
    public UserModel User { get; set; }

    public GroupModel Group { get; set; }

    public bool IsAdmin { get; set; }

    public bool CanView { get; set; }

    public bool CanEdit { get; set; }

    public bool CanRemove { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if ((User == null && Group == null) || (User != null && Group != null))
            return new[]
            {
                new ValidationResult("The permission must only belong to a group or user.",
                    new[] { nameof(User), nameof(Group) })
            };
        
        return new[] { ValidationResult.Success };
    }
}