namespace Mimisbrunnr.Web.Host.Configuration
{
    public class OpenIdConfiguration
    {
        public string Authority { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string ResponseType { get; set; }

        public string[] Scope { get; set; }
    }
}