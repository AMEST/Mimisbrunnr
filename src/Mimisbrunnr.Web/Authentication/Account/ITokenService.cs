using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Authentication.Account;

public interface ITokenService
{
    Task<IEnumerable<TokenModel>> GetUserTokens(UserInfo requestBy);

    Task<TokenCreateResult> CreateUserToken(TokenCreateRequest request, UserInfo createdBy);

    Task Revoke(string tokenId, UserInfo revokedBy);
}