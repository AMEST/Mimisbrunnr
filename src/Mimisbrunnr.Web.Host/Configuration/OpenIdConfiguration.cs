namespace Mimisbrunnr.Web.Host.Configuration
{
    /// <summary>
    /// Configuration settings for OpenID Connect authentication
    /// </summary>
    public class OpenIdConfiguration
    {
        /// <summary>
        /// Gets or sets the URL of the OpenID Connect authority
        /// </summary>
        public string Authority { get; set; }

        /// <summary>
        /// Gets or sets the client identifier used for authentication
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the client secret used for authentication
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the response type requested from the OpenID Connect provider
        /// </summary>
        public string ResponseType { get; set; }

        /// <summary>
        /// Gets or sets the scopes requested from the OpenID Connect provider
        /// </summary>
        public string[] Scope { get; set; }
    }
}