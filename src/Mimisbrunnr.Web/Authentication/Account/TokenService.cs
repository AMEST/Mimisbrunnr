using Mimisbrunnr.Users;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Mapping;
using Mimisbrunnr.Wiki.Contracts;

namespace Mimisbrunnr.Web.Authentication.Account;

internal class TokenService : ITokenService
{
    private readonly ISecurityTokenService _securityTokenService;
    private readonly IUserManager _userManager;

    public TokenService(
        ISecurityTokenService securityTokenService,
        IUserManager userManager
    )
    {
        _securityTokenService = securityTokenService;
        _userManager = userManager;
    }


    public async Task<TokenCreateResult> CreateUserToken(TokenCreateRequest request, UserInfo createdBy)
    {
        var user = await _userManager.GetByEmail(createdBy.Email);
        if (user is null)
            return null;

        var token = await _securityTokenService.GenerateAccessToken(user, request.Lifetime);
        return new TokenCreateResult()
        {
            Token = token
        };
    }

    public async Task<IEnumerable<TokenModel>> GetUserTokens(UserInfo requestBy)
    {
        var user = await _userManager.GetByEmail(requestBy.Email);
        if (user is null)
            return Enumerable.Empty<TokenModel>();

        var tokens = await _securityTokenService.GetUserTokens(user);
        return TokenMapper.Instance.ToModel(tokens);
    }

    public async Task Revoke(string tokenId, UserInfo revokedBy)
    {
        var user = await _userManager.GetByEmail(revokedBy.Email);
        if (user is null)
            return;

        await _securityTokenService.RevokeToken(tokenId, user);
    }
}