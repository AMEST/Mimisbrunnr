using System.Net.Http.Headers;
using System.Text;
using Mimisbrunnr.Web.Host.Configuration;
using Prometheus;
using Prometheus.DotNetRuntime;
using Skidbladnir.Modules;

namespace Mimisbrunnr.Web.Host.Services.Metrics
{
    internal class MetricsModule : RunnableModule
    {
        private MetricsConfiguration _configuration;
        private IDisposable _collector;

        public override void Configure(IServiceCollection services)
        {
            _configuration = Configuration.Get<MetricsConfiguration>();
            services.AddSingleton(_configuration);
        }

        public override Task StartAsync(IServiceProvider provider, CancellationToken cancellationToken = default)
        {

            var logger = provider.GetService<ILogger<MetricsModule>>();

            if (!_configuration.Enabled)
                return Task.CompletedTask;

            if (_configuration.Enabled && _configuration.BasicAuth)
                if (string.IsNullOrEmpty(_configuration.Username) || string.IsNullOrEmpty(_configuration.Password))
                    throw new ArgumentNullException("Username or password can't be null or empty when metrics enablend and authroization enabled too.");

            var environment = provider.GetService<IHostEnvironment>();
            Prometheus.Metrics.DefaultRegistry.SetStaticLabels(new Dictionary<string, string>
            {
                { "environment", environment.EnvironmentName },
                { "hostname", Environment.MachineName },
                { "application", "Mimisbrunnr" }
            });
            
            StartCollectingMetrics(logger);

            if (_configuration.PushGatewayEnabled)
            {
                if(string.IsNullOrEmpty(_configuration.PushGatewayEndpoint))
                    throw new ArgumentNullException("PushGatewayEndpoint can't be null or empty when PushGateway enablend");
                
                StartMetricsPusher();
            }

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _collector?.Dispose();
            return base.StopAsync(cancellationToken);
        }

        private void StartCollectingMetrics(ILogger<MetricsModule> logger)
        {
            var builder = DotNetRuntimeStatsBuilder.Customize()
                    .WithContentionStats(CaptureLevel.Informational)
                    .WithGcStats(CaptureLevel.Verbose)
                    .WithThreadPoolStats(CaptureLevel.Informational)
                    .WithExceptionStats(CaptureLevel.Errors)
                    .WithJitStats()
                    .WithErrorHandler(ex => logger.LogError(ex, "Unexpected exception occurred in prometheus-net.DotNetRuntime"));

            logger.LogInformation("Starting prometheus-net.DotNetRuntime...");

            _collector = builder.StartCollecting();
        }

        private void StartMetricsPusher()
        {
            var httpClient = new HttpClient();

            var uri = new Uri(_configuration.PushGatewayEndpoint);
            if(!string.IsNullOrEmpty(uri.UserInfo))
            {
                var headerValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(uri.UserInfo));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", headerValue);
            }
            
            var pusher = new MetricPusher(new MetricPusherOptions
            {
                Endpoint = _configuration.PushGatewayEndpoint,
                Job = _configuration.PushGatewayJob,
                HttpClientProvider = () => httpClient
            });

            pusher.Start();
        }
    }
}