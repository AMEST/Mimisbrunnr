using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Mimisbrunnr.Web.Infrastructure;

namespace Mimisbrunnr.Web.Host.Middlewares;

/// <summary>
/// Patches the SPA index.html title and manifest.json name/short_name
/// so that they always match the application title configured in the database.
/// </summary>
internal class SpaBrandingMiddleware
{
    private const string StaticRootRelativePath = "ClientApp/dist";
    private const string IndexHtmlFileName = "index.html";
    private const string ManifestFileName = "manifest.json";
    private const string DefaultTitle = "Mimisbrunnr wiki";

    private static readonly Regex TitleTag = new(
        @"<title[^>]*>.*?</title>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
    private static readonly ConcurrentDictionary<string, string> FileCache = new();

    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;

    public SpaBrandingMiddleware(RequestDelegate next, IWebHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context, IApplicationConfigurationManager configurationManager)
    {
        if (!HttpMethods.IsGet(context.Request.Method))
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;
        if (IsIndexRequest(path))
        {
            await ServePatchedIndexAsync(context, configurationManager);
            return;
        }

        if (IsManifestRequest(path))
        {
            await ServePatchedManifestAsync(context, configurationManager);
            return;
        }

        await _next(context);
    }

    private static bool IsIndexRequest(string path)
        => path == "/" || path.Equals("/index.html", StringComparison.OrdinalIgnoreCase);

    private static bool IsManifestRequest(string path)
        => path.Equals("/manifest.json", StringComparison.OrdinalIgnoreCase);

    private async Task ServePatchedIndexAsync(HttpContext context, IApplicationConfigurationManager configurationManager)
    {
        var html = await GetFileContentAsync(IndexHtmlFileName);
        if (html is null)
        {
            await _next(context);
            return;
        }

        var title = await GetTitleAsync(configurationManager);
        html = TitleTag.Replace(html, $"<title>{WebUtility.HtmlEncode(title)}</title>");
        await WriteAsync(context, "text/html; charset=utf-8", html);
    }

    private async Task ServePatchedManifestAsync(HttpContext context, IApplicationConfigurationManager configurationManager)
    {
        var manifest = await GetFileContentAsync(ManifestFileName);
        if (manifest is null)
        {
            await _next(context);
            return;
        }

        var title = await GetTitleAsync(configurationManager);
        if (JsonNode.Parse(manifest) is JsonObject json)
        {
            json["name"] = title;
            json["short_name"] = title;
            manifest = json.ToJsonString();
        }

        await WriteAsync(context, "application/manifest+json; charset=utf-8", manifest);
    }

    private static async Task<string> GetTitleAsync(IApplicationConfigurationManager configurationManager)
    {
        var configuration = await configurationManager.Get();
        var title = configuration?.Title;
        return string.IsNullOrWhiteSpace(title) ? DefaultTitle : title.Trim();
    }

    private Task<string> GetFileContentAsync(string fileName)
    {
        var filePath = Path.Combine(_environment.ContentRootPath, StaticRootRelativePath, fileName);
        if (!File.Exists(filePath))
            return Task.FromResult<string>(null);
        return Task.FromResult(FileCache.GetOrAdd(filePath, path => File.ReadAllText(path, Encoding.UTF8)));
    }

    private static async Task WriteAsync(HttpContext context, string contentType, string content)
    {
        context.Response.ContentType = contentType;
        await context.Response.WriteAsync(content, Encoding.UTF8);
    }
}
