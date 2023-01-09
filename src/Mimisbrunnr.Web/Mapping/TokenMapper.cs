using Mimisbrunnr.Web.Infrastructure.Contracts;
using Mimisbrunnr.Web.Authentication.Account;
using Riok.Mapperly.Abstractions;

namespace Mimisbrunnr.Web.Mapping;

[Mapper]
public static partial class TokenMapper
{

    public static partial TokenModel ToModel(this UserToken token);

    public static IEnumerable<TokenModel> ToModel(this IEnumerable<UserToken> tokens)
    {
        return tokens?.Select(ToModel);
    }
}