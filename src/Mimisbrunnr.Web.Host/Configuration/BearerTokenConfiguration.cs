namespace Mimisbrunnr.Web.Host.Configuration;

public class BearerTokenConfiguration
{
    public string SymmetricKey { get; set; }

    public string Issuer { get; set; } = "Mimisbrunnr";

    public string Audience { get; set; } = "WebApi";
}