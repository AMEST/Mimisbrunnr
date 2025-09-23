namespace Mimisbrunnr.Integration.User;

/// <summary>
/// Extends the base UserModel with additional view-specific properties.
/// This model is used when displaying user information in the Mimisbrunnr UI.
/// </summary>
public class UserViewModel : UserModel
{
    /// <summary>
    /// Indicates whether the user has administrative privileges in the system.
    /// Admin users have access to all features and can manage system settings.
    /// </summary>
    public bool IsAdmin { get; set; }

    /// <summary>
    /// Indicates whether the user account is active and enabled.
    /// Disabled accounts cannot access the system.
    /// </summary>
    public bool Enable { get; set; }
}
