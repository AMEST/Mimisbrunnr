using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Mimisbrunnr.Web.Host.Configuration;
using Skidbladnir.Modules;

namespace Mimisbrunnr.Web.Host.Services;

public class AspNetModule : Module
{
    private const string JwtOrCookeSchemeName = "JWT_OR_COOKIE";
    public override void Configure(IServiceCollection services)
    {
        var bearerConfiguration = Configuration.Get<BearerTokenConfiguration>();
        if(string.IsNullOrEmpty(bearerConfiguration.SymmetricKey))
            throw new ApplicationException("Bearer:SymmetricKey can't be null or empty");

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtOrCookeSchemeName;
                options.DefaultChallengeScheme = JwtOrCookeSchemeName;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = "Mimisbrunnr";
                options.Events.OnRedirectToAccessDenied =
                    options.Events.OnRedirectToLogin = c =>
                    {
                        c.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.FromResult<object>(null);
                    };
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = TokenValidationParametersFactory.Create(bearerConfiguration);
            })
            .AddOpenIdConnect(options =>
            {
                Configuration.AppConfiguration.GetSection("Openid").Bind(options);
            })
            .AddPolicyScheme(JwtOrCookeSchemeName, JwtOrCookeSchemeName, options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    // filter by auth type
                    string authorization = context.Request.Headers[HeaderNames.Authorization];
                    if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
                        return JwtBearerDefaults.AuthenticationScheme;

                    // otherwise always check for cookie auth
                    return CookieAuthenticationDefaults.AuthenticationScheme;
                };
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
        services.AddSwaggerGen(options => {
            options.MapType<TimeSpan>(() => new OpenApiSchema(){
                Type = "string",
                Example = new OpenApiString("02:00:00")
            });
        });
    }
}