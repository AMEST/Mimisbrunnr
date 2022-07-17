using Mimisbrunnr.Web.Host.Configuration;
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

            if(!_configuration.Enabled)
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

            var builder = DotNetRuntimeStatsBuilder.Customize()
                    .WithContentionStats(CaptureLevel.Informational)
                    .WithGcStats(CaptureLevel.Verbose)
                    .WithThreadPoolStats(CaptureLevel.Informational)
                    .WithExceptionStats(CaptureLevel.Errors)
                    .WithJitStats()
                    .WithErrorHandler(ex => logger.LogError(ex, "Unexpected exception occurred in prometheus-net.DotNetRuntime"));

            logger.LogInformation("Starting prometheus-net.DotNetRuntime...");

            _collector = builder.StartCollecting();

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _collector?.Dispose();
            return base.StopAsync(cancellationToken);
        }
    }
}