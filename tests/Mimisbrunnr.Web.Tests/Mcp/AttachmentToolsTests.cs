using System.Text;
using System.Text.Json;
using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Web.Mcp;
using Mimisbrunnr.Web.Mcp.Tools;
using Mimisbrunnr.Web.Wiki;
using ModelContextProtocol;

namespace Mimisbrunnr.Web.Tests.Mcp;

public class AttachmentToolsTests
{
    [Fact]
    public async Task Should_ReturnBase64Content_WhenGetAttachmentInvoked()
    {
        var attachments = A.Fake<IAttachmentService>();
        A.CallTo(() => attachments.GetAttachmentContent("page-1", "a.txt", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .Returns(Task.FromResult<Stream>(new MemoryStream(Encoding.UTF8.GetBytes("hello"))));
        var tools = new AttachmentTools(attachments, new McpServerConfiguration(),
            McpTestFactory.CreateContext(), McpTestFactory.CreateExecutor());

        var json = await tools.GetAttachment("page-1", "a.txt");

        using var document = JsonDocument.Parse(json);
        document.RootElement.GetProperty("encoding").GetString().Should().Be("base64");
        document.RootElement.GetProperty("content").GetString()
            .Should().Be(Convert.ToBase64String(Encoding.UTF8.GetBytes("hello")));
    }

    [Fact]
    public async Task Should_ReturnAttachmentTooLarge_WhenUploadExceedsLimit()
    {
        var attachments = A.Fake<IAttachmentService>();
        var tools = new AttachmentTools(attachments, new McpServerConfiguration { MaxAttachmentBytes = 4 },
            McpTestFactory.CreateContext(), McpTestFactory.CreateExecutor());
        var content = Convert.ToBase64String(Encoding.UTF8.GetBytes("aaaaa"));

        var exception = await Assert.ThrowsAsync<McpException>(() => tools.UploadAttachment("page-1", "a.bin", content));

        exception.Message.Should().StartWith("attachment_too_large");
        A.CallTo(() => attachments.Upload(A<string>._, A<Stream>._, A<string>._, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_ReturnInvalidArgument_WhenUploadContentIsNotBase64()
    {
        var attachments = A.Fake<IAttachmentService>();
        var tools = new AttachmentTools(attachments, new McpServerConfiguration(),
            McpTestFactory.CreateContext(), McpTestFactory.CreateExecutor());

        var exception = await Assert.ThrowsAsync<McpException>(() => tools.UploadAttachment("page-1", "a.bin", "not-base64!"));

        exception.Message.Should().StartWith("invalid_argument");
    }

    [Fact]
    public async Task Should_UploadAttachment_WhenContentIsValid()
    {
        var attachments = A.Fake<IAttachmentService>();
        var tools = new AttachmentTools(attachments, new McpServerConfiguration(),
            McpTestFactory.CreateContext(), McpTestFactory.CreateExecutor());
        var content = Convert.ToBase64String(Encoding.UTF8.GetBytes("data"));

        var result = await tools.UploadAttachment("page-1", "a.bin", content);

        result.Should().Contain("ok");
        A.CallTo(() => attachments.Upload("page-1", A<Stream>._, "a.bin", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .MustHaveHappenedOnceExactly();
    }
}
