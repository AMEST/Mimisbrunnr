using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Web.User;

public class UserModel
{
    [Required]
    public string Email { get; set; }

    public string Name { get; set; }
    
    public string AvatarUrl { get; set; }
}