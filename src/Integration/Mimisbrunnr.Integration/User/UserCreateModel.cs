using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Integration.User;

public class UserCreateModel
{
    [Required]
    public string Email { get; set; }
    [Required]
    public string Name { get; set; }

    public string AvatarUrl { get; set; }
}