using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Mimisbrunnr.Web.Host.Configuration;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Infrastructure.Contracts;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Web.Host.Services;

internal class SecurityTokenService : ISecurityTokenService
{
    private const string TokenIdClaim = "TokenId";
    private const string SystemTokenClaim = "SystemToken";
    private readonly BearerTokenConfiguration _configuration;
    private readonly IRepository<UserToken> _userTokenRepository;
    private readonly ILogger<SecurityTokenService> _logger;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();
    private readonly SymmetricSecurityKey _key;

    public SecurityTokenService(
        BearerTokenConfiguration configuration,
        IRepository<UserToken> userTokenRepository,
        ILogger<SecurityTokenService> logger
    )
    {
        _configuration = configuration;
        _userTokenRepository = userTokenRepository;
        _logger = logger;
        var key = Encoding.ASCII.GetBytes(configuration.SymmetricKey);
        _key = new SymmetricSecurityKey(key);
    }

    public async Task<bool> EnsureTokenNotRevoked(string token)
    {
        var principal = GetPrincipal(token);
        if (principal is null)
            return false;

        var tokenId = principal.FindFirstValue(TokenIdClaim);
        var systemTokenString = principal.FindFirstValue(SystemTokenClaim);
        if (!string.IsNullOrEmpty(systemTokenString) && bool.TryParse(systemTokenString, out var systemToken) && systemToken)
            return true;

        var hasRevoked = await _userTokenRepository.GetAll().AnyAsync(x => x.Id == tokenId && x.Revoked);

        return !hasRevoked;
    }

    public async Task<string> GenerateAccessToken(Users.User user, TimeSpan? tokenLifeTime = null, bool systemToken = false)
    {
        var lifetime = tokenLifeTime ?? TimeSpan.FromDays(30);
        var userToken = new UserToken
        {
            Created = DateTime.UtcNow,
            Expired = DateTime.UtcNow + lifetime,
            UserId = user.Id,
        };
        if (!systemToken)
            await _userTokenRepository.Create(userToken);
        else
            userToken.Id = Guid.Empty.ToString();

        var principal = CreatePrincipal(user, userToken.Id, systemToken);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _configuration.Issuer,
            Audience = _configuration.Audience,
            Subject = principal.Identities.FirstOrDefault(),
            Expires = userToken.Expired,
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256)
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        return _tokenHandler.WriteToken(token);
    }

    public async Task<IEnumerable<UserToken>> GetUserTokens(Users.User user)
    {
        var userTokens = await _userTokenRepository
            .GetAll()
            .Where(x => x.UserId == user.Id)
            .ToArrayAsync();
        return userTokens;
    }

    public async Task RevokeToken(string tokenId, Users.User user)
    {
        var userToken = await _userTokenRepository
            .GetAll()
            .FirstOrDefaultAsync(x => x.Id == tokenId && x.UserId == user.Id);
        userToken.Revoked = true;
        await _userTokenRepository.Update(userToken);
    }

    private ClaimsPrincipal CreatePrincipal(Users.User user, string tokenId, bool systemToken)
    {
        return new ClaimsPrincipal(
            new ClaimsIdentity(
            [
                new(ClaimTypes.NameIdentifier, user.Email),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.Name),
                new(ClaimTypes.Role, user.Role.ToString()),
                new(TokenIdClaim, tokenId),
                new(SystemTokenClaim, $"{systemToken}")
            ]));
    }

    public ClaimsPrincipal GetPrincipal(string token)
    {
        var tokenValidation = TokenValidationParametersFactory.Create(_configuration);
        try
        {
            return _tokenHandler.ValidateToken(token, tokenValidation, out var securityToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error validation token");
            return null;
        }
    }
}