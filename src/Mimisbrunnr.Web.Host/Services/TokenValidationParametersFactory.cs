using System.Text;
using Microsoft.IdentityModel.Tokens;
using Mimisbrunnr.Web.Host.Configuration;

namespace Mimisbrunnr.Web.Host.Services;

internal static class TokenValidationParametersFactory
{
    public static TokenValidationParameters Create(BearerTokenConfiguration configuration)
    {
        return new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidIssuer = configuration.Issuer,
            ValidateAudience = false,
            ValidAudience = configuration.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.SymmetricKey))
        };
    }
}