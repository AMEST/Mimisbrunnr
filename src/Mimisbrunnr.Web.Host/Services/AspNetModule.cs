using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Skidbladnir.Modules;

namespace Mimisbrunnr.Web.Host.Services;

public class AspNetModule : Module
{
    public override void Configure(IServiceCollection services)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.RequireAuthenticatedSignIn = false;
            })
            .AddCookie(options =>
            {
                options.Cookie.Name = "Mimisbunnr";
                options.Events.OnRedirectToAccessDenied =
                    options.Events.OnRedirectToLogin = c =>
                    {
                        c.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.FromResult<object>(null);
                    };
            })
            .AddOpenIdConnect(options =>
            {
                Configuration.AppConfiguration.GetSection("Openid").Bind(options);
            });
        services.AddMemoryCache();
        services.AddControllers()
            .AddJsonOptions(x =>
            {
                x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
        services.AddDataProtection()
            .SetApplicationName("Mimisbrunnr");
        // In production, the Vue files will be served from this directory
        services.AddSpaStaticFiles(configuration =>
        {
            configuration.RootPath = "ClientApp/dist";
        });
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }
}