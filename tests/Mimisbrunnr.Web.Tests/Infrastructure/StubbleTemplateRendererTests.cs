using FluentAssertions;
using Mimisbrunnr.Web.Infrastructure;

namespace Mimisbrunnr.Web.Tests.Infrastructure;

public class StubbleTemplateRendererTests
{
    private readonly StubbleTemplateRenderer _renderer = new();

    [Fact]
    public async Task Should_RenderTemplate_WithVariable()
    {
        var result = await _renderer.Render("Hello {{name}}!", new Dictionary<string, object> { ["name"] = "World" });

        result.Should().Be("Hello World!");
    }

    [Fact]
    public async Task Should_RenderEmpty_WhenNoVariablesProvided()
    {
        var result = await _renderer.Render("Hello {{name}}!", new Dictionary<string, object>());

        result.Should().Be("Hello !");
    }

    [Fact]
    public async Task Should_BeCaseInsensitive_WhenLookingUpKeys()
    {
        var result = await _renderer.Render("{{Name}}", new Dictionary<string, object> { ["name"] = "value" });

        result.Should().Be("value");
    }

    [Fact]
    public async Task Should_RenderUrlEncodeHelper()
    {
        var result = await _renderer.Render("{{#UrlEncode}}hello world{{/UrlEncode}}", new Dictionary<string, object>());

        result.Should().Be("hello+world");
    }

    [Fact]
    public async Task Should_RenderHtmlEncodeHelper()
    {
        var result = await _renderer.Render("{{#HtmlEncode}}<script>alert(1)</script>{{/HtmlEncode}}", new Dictionary<string, object>());

        result.Should().Be("&lt;script&gt;alert(1)&lt;/script&gt;");
    }

    [Fact]
    public async Task Should_RenderToLowerHelper()
    {
        var result = await _renderer.Render("{{#ToLower}}HELLO{{/ToLower}}", new Dictionary<string, object>());

        result.Should().Be("hello");
    }

    [Fact]
    public async Task Should_RenderToUpperHelper()
    {
        var result = await _renderer.Render("{{#ToUpper}}hello{{/ToUpper}}", new Dictionary<string, object>());

        result.Should().Be("HELLO");
    }

    [Fact]
    public async Task Should_RenderUuidHelper()
    {
        var result = await _renderer.Render("{{#Uuid}}x{{/Uuid}}", new Dictionary<string, object>());

        result.Should().NotBeNullOrEmpty();
        result.Should().HaveLength(32);
    }

    [Fact]
    public async Task Should_GenerateUniqueUuids()
    {
        var r1 = await _renderer.Render("{{#Uuid}}x{{/Uuid}}", new Dictionary<string, object>());
        var r2 = await _renderer.Render("{{#Uuid}}x{{/Uuid}}", new Dictionary<string, object>());

        r1.Should().NotBe(r2);
    }
}
