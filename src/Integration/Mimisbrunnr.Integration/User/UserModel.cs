using System.ComponentModel.DataAnnotations;

namespace Mimisbrunnr.Integration.User;

/// <summary>
/// Represents a user entity in the Mimisbrunnr system.
/// This model is used for transferring user data between the application layers.
/// </summary>
public class UserModel
{
    [Required]
    /// <summary>
    /// The unique email address identifying the user.
    /// This is used for authentication and must be a valid email format.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// The display name of the user, shown throughout the application.
    /// This can be changed by the user in their profile settings.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The URL pointing to the user's profile picture.
    /// This is automatically generated but can be customized by the user.
    /// </summary>
    public string AvatarUrl { get; set; }
}
