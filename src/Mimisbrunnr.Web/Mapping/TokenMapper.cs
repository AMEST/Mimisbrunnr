using Mimisbrunnr.Web.Infrastructure.Contracts;
using Mimisbrunnr.Web.Authentication.Account;
using Riok.Mapperly.Abstractions;

namespace Mimisbrunnr.Web.Mapping;

[Mapper]
public partial class TokenMapper
{

    public partial TokenModel ToModel(UserToken token);

    public IEnumerable<TokenModel> ToModel(IEnumerable<UserToken> tokens)
    {
        return tokens?.Select(ToModel);
    }
}