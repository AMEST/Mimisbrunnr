namespace Mimisbrunnr.Web.Authentication.Account;

/// <summary>
/// Request parameters for creating a token
/// </summary>
public class TokenCreateRequest
{
    /// <summary>
    /// Optional lifetime of the token
    /// </summary>
    public TimeSpan? Lifetime { get; set; }
}