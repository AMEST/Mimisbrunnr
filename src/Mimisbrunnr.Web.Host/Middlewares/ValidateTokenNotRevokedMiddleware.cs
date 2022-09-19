using Microsoft.Net.Http.Headers;
using Mimisbrunnr.Web.Infrastructure;

namespace Mimisbrunnr.Web.Host.Middlewares;

internal class ValidateTokenNotRevokedMiddleware
{

    private readonly RequestDelegate _next;

    public ValidateTokenNotRevokedMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ISecurityTokenService securityTokenService)
    {

        string authorization = context.Request.Headers[HeaderNames.Authorization];
        if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
        {
            await _next(context);
            return;
        }

        if(await securityTokenService.EnsureTokenNotRevoked(authorization.Replace("Bearer ", "")))
        {
            await _next(context);
            return;
        }

        context.Response.StatusCode = 401;
    }
}