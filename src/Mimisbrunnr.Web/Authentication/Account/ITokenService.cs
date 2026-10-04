using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Authentication.Account;

/// <summary>
/// Service for managing user access tokens
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Get all tokens for a user
    /// </summary>
    /// <param name="requestBy">User requesting the tokens</param>
    /// <returns>List of user tokens</returns>
    Task<IEnumerable<TokenModel>> GetUserTokens(UserInfo requestBy);

    /// <summary>
    /// Create a new token for a user
    /// </summary>
    /// <param name="request">Token creation parameters</param>
    /// <param name="createdBy">User performing the creation</param>
    /// <returns>The created token</returns>
    Task<TokenCreateResult> CreateUserToken(TokenCreateRequest request, UserInfo createdBy);

    /// <summary>
    /// Revoke a token
    /// </summary>
    /// <param name="tokenId">ID of the token to revoke</param>
    /// <param name="revokedBy">User performing the revocation</param>
    Task Revoke(string tokenId, UserInfo revokedBy);
}