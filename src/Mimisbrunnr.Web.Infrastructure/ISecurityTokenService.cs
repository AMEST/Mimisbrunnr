using System.Security.Claims;
using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Infrastructure.Contracts;

namespace Mimisbrunnr.Web.Infrastructure;

/// <summary>
///     Service for working with user tokens
/// </summary>
public interface ISecurityTokenService
{
    /// <summary>
    ///     Get all user tokens
    /// </summary>
    Task<IEnumerable<UserToken>> GetUserTokens(User user);

    /// <summary>
    ///     Generate access token
    /// </summary>
    Task<string> GenerateAccessToken(User user, TimeSpan? tokenLifeTime = null, bool systemToken = false);

    /// <summary>
    ///     Ensure token of the already authenticated principal is not revoked.
    ///     The result is cached, so revocation takes effect with a short delay.
    /// </summary>
    Task<bool> EnsureTokenNotRevoked(ClaimsPrincipal principal);

    /// <summary>
    ///     Revoke user token by token id
    /// </summary>
    Task RevokeToken(string tokenId, User user);
}
