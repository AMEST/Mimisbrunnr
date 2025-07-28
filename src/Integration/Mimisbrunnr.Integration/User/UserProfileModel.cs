namespace Mimisbrunnr.Integration.User;

/// <summary>
/// Extends the base UserModel with additional profile details.
/// This model is used to manage and display extended user information
/// in the Mimisbrunnr application.
/// </summary>
public class UserProfileModel : UserModel
{
    /// <summary>
    /// The user's personal or professional website URL.
    /// This is displayed in the user's public profile and can be left blank.
    /// </summary>
    public string Website { get; set; }

    /// <summary>
    /// The user's current job title or position.
    /// Used to display professional information in the user profile.
    /// </summary>
    public string Post { get; set; }

    /// <summary>
    /// The department or team the user belongs to within their organization.
    /// Helps identify the user's area of responsibility.
    /// </summary>
    public string Department { get; set; }

    /// <summary>
    /// The name of the company or organization the user is associated with.
    /// Used to display professional affiliation in the user profile.
    /// </summary>
    public string Organization { get; set; }

    /// <summary>
    /// The user's geographic location, typically city and country.
    /// Helps identify where the user is based.
    /// </summary>
    public string Location { get; set; }
}
