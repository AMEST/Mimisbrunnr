namespace Mimisbrunnr.Web.Authentication.Account;

/// <summary>
/// Represents a user access token
/// </summary>
public class TokenModel
{
    /// <summary>
    /// Unique identifier of the token
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Date and time when the token was created
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Date and time when the token expires
    /// </summary>
    public DateTime Expired { get; set; }

    /// <summary>
    /// Indicates whether the token has been revoked
    /// </summary>
    public bool Revoked { get; set; }
}