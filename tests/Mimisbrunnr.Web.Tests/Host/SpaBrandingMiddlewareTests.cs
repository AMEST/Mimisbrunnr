using System.Text.Json;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Mimisbrunnr.Web.Host.Middlewares;
using Mimisbrunnr.Web.Infrastructure;
using Mimisbrunnr.Web.Infrastructure.Contracts;

namespace Mimisbrunnr.Web.Tests.Host;

public class SpaBrandingMiddlewareTests : IDisposable
{
    private readonly string _rootPath;

    public SpaBrandingMiddlewareTests()
    {
        _rootPath = Path.Combine(Path.GetTempPath(), "mimisbrunnr-spa-branding-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(_rootPath, "ClientApp", "dist"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_rootPath))
            Directory.Delete(_rootPath, true);
    }

    [Fact]
    public async Task Should_PatchTitle_WhenIndexRequested()
    {
        WriteIndex("<!DOCTYPE html><html><head><title></title></head><body></body></html>");
        var (middleware, config) = Create("My Wiki");
        var context = Context("/");

        await middleware.InvokeAsync(context, config);

        var body = await ReadBody(context);
        body.Should().Contain("<title>My Wiki</title>");
        context.Response.ContentType.Should().Be("text/html; charset=utf-8");
    }

    [Fact]
    public async Task Should_PatchManifestNameAndShortName_WhenManifestRequested()
    {
        WriteManifest("{\"name\":\"mimisbrunnr\",\"short_name\":\"mimisbrunnr\",\"start_url\":\"./\",\"display\":\"standalone\"}");
        var (middleware, config) = Create("My Wiki");
        var context = Context("/manifest.json");

        await middleware.InvokeAsync(context, config);

        var body = await ReadBody(context);
        using var document = JsonDocument.Parse(body);
        document.RootElement.GetProperty("name").GetString().Should().Be("My Wiki");
        document.RootElement.GetProperty("short_name").GetString().Should().Be("My Wiki");
        document.RootElement.GetProperty("start_url").GetString().Should().Be("./");
        context.Response.ContentType.Should().Be("application/manifest+json; charset=utf-8");
    }

    [Fact]
    public async Task Should_HtmlEncodeTitle_WhenTitleContainsMarkup()
    {
        WriteIndex("<html><head><title></title></head></html>");
        var (middleware, config) = Create("<script>alert('x')</script>");
        var context = Context("/index.html");

        await middleware.InvokeAsync(context, config);

        var body = await ReadBody(context);
        body.Should().Contain("<title>&lt;script&gt;alert(&#39;x&#39;)&lt;/script&gt;</title>");
    }

    [Fact]
    public async Task Should_UseDefaultTitle_WhenConfigurationIsMissing()
    {
        WriteIndex("<html><head><title></title></head></html>");
        var (middleware, config) = Create(null);
        var context = Context("/");

        await middleware.InvokeAsync(context, config);

        var body = await ReadBody(context);
        body.Should().Contain("<title>Mimisbrunnr wiki</title>");
    }

    [Fact]
    public async Task Should_InvokeNext_WhenPathIsNotBranded()
    {
        WriteIndex("<html><head><title></title></head></html>");
        var nextCalled = false;
        var environment = Environment();
        var config = Configuration("My Wiki");
        var middleware = new SpaBrandingMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, environment);

        await middleware.InvokeAsync(Context("/js/app.js"), config);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Should_InvokeNext_WhenIndexFileIsMissing()
    {
        var nextCalled = false;
        var environment = Environment();
        var config = Configuration("My Wiki");
        var middleware = new SpaBrandingMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, environment);

        await middleware.InvokeAsync(Context("/"), config);

        nextCalled.Should().BeTrue();
    }

    private void WriteIndex(string content)
        => File.WriteAllText(Path.Combine(_rootPath, "ClientApp", "dist", "index.html"), content);

    private void WriteManifest(string content)
        => File.WriteAllText(Path.Combine(_rootPath, "ClientApp", "dist", "manifest.json"), content);

    private (SpaBrandingMiddleware Middleware, IApplicationConfigurationManager Config) Create(string title)
        => (new SpaBrandingMiddleware(_ => Task.CompletedTask, Environment()), Configuration(title));

    private IWebHostEnvironment Environment()
    {
        var environment = A.Fake<IWebHostEnvironment>();
        A.CallTo(() => environment.ContentRootPath).Returns(_rootPath);
        return environment;
    }

    private static IApplicationConfigurationManager Configuration(string title)
    {
        var config = A.Fake<IApplicationConfigurationManager>();
        A.CallTo(() => config.Get()).Returns(Task.FromResult(title is null ? null : new ApplicationConfiguration { Title = title }));
        return config;
    }

    private static DefaultHttpContext Context(string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<string> ReadBody(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }
}
