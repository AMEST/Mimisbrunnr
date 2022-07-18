namespace Mimisbrunnr.Web.Host.Configuration
{

    /// <summary>
    /// Metrics configurations
    /// </summary>    
    public class MetricsConfiguration
    {
        /// <summary>
        /// Is metrics enabled flag
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Endpoint url with prometheus metrics
        /// </summary>
        public string Endpoint { get; set; } = "/api/metrics";

        /// <summary>
        /// Is basic authentication enabled
        /// </summary>
        public bool BasicAuth { get; set; }

        /// <summary>
        /// Basic username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Basic password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        ///     Push metrics to PushGateway enabled flag
        /// </summary>
        public bool PushGatewayEnabled { get; set; }

        /// <summary>
        ///     PushGateway endpoint
        /// </summary>
        public string PushGatewayEndpoint {get; set;}

        /// <summary>
        ///     PushGateway job name
        /// </summary>
        public string PushGatewayJob { get; set; } = "AppTelemetry";
    }
}