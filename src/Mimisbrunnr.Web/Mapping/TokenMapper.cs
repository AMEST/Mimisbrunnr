using Mimisbrunnr.Web.Infrastructure.Contracts;
using Mimisbrunnr.Web.Authentication.Account;
using Riok.Mapperly.Abstractions;

namespace Mimisbrunnr.Web.Mapping;

/// <summary>
/// Maps user token entities to their web models
/// </summary>
[Mapper]
public static partial class TokenMapper
{
    /// <summary>
    /// Maps a user token entity to a token model
    /// </summary>
    /// <param name="token">User token entity to map</param>
    /// <returns>The mapped token model</returns>
    [MapperIgnoreSource(nameof(UserToken.UserId))]
    public static partial TokenModel ToModel(this UserToken token);

    /// <summary>
    /// Maps a collection of user token entities to token models
    /// </summary>
    /// <param name="tokens">User token entities to map</param>
    /// <returns>The mapped token models</returns>
    public static IEnumerable<TokenModel> ToModel(this IEnumerable<UserToken> tokens)
    {
        return tokens?.Select(ToModel);
    }
}