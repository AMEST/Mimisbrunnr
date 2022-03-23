using Skidbladnir.Modules;

namespace Mimisbrunnr.Web.Host.Services;

public class AspNetModule : Module
{
    public override void Configure(IServiceCollection services)
    { 
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }
}