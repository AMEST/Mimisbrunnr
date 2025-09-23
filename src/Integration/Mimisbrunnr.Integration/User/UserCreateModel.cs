using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Integration.User;

/// <summary>
/// Represents the data required to create a new user in the Mimisbrunnr system.
/// This model is used during user registration and account creation processes.
/// </summary>
public class UserCreateModel
{
    [Required]
    /// <summary>
    /// The unique email address for the new user account.
    /// This will be used for authentication and must be unique across the system.
    /// </summary>
    public string Email { get; set; }
    [Required]
    /// <summary>
    /// The display name for the new user account.
    /// This will be visible throughout the application and can be changed later.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Optional URL for the user's profile picture.
    /// If not provided, a default avatar will be generated.
    /// </summary>
    public string AvatarUrl { get; set; }
}
