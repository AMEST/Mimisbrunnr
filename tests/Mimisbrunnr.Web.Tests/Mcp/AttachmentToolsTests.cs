using System.Text;
using System.Text.Json;
using FakeItEasy;
using FluentAssertions;
using Mimisbrunnr.Wiki.Contracts;
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
        A.CallTo(() => attachments.GetAttachmentContent("page-1", "a.txt", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._, A<long>._))
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
    public async Task ShouldThrow_AttachmentTooLarge_WhenStorageRejectsDownload()
    {
        var attachments = A.Fake<IAttachmentService>();
        A.CallTo(() => attachments.GetAttachmentContent("page-1", "a.bin", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._, 4))
            .Throws(new AttachmentTooLargeException(4));
        var context = McpTestFactory.CreateContext();
        var tools = new AttachmentTools(attachments, new McpServerConfiguration { MaxAttachmentBytes = 4 },
            context, McpTestFactory.CreateExecutor(context));

        var exception = await Assert.ThrowsAsync<McpException>(() => tools.GetAttachment("page-1", "a.bin"));

        exception.Message.Should().StartWith("attachment_too_large:");
        A.CallTo(() => attachments.GetAttachmentContent("page-1", "a.bin", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._, 4))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => attachments.GetAttachmentContent(A<string>._, A<string>._, A<Mimisbrunnr.Wiki.Contracts.UserInfo>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task ShouldThrow_AttachmentTooLargeWithoutReading_WhenSeekableDownloadExceedsLimit()
    {
        using var stream = new TrackingStream([1, 2, 3, 4, 5]);
        var tools = CreateDownloadTools(stream, 4);

        var exception = await Assert.ThrowsAsync<McpException>(() => tools.GetAttachment("page-1", "a.bin"));

        exception.Message.Should().StartWith("attachment_too_large:");
        stream.ReadCalls.Should().Be(0);
        stream.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldThrow_AttachmentTooLarge_WhenNonSeekableDownloadExceedsLimit()
    {
        using var stream = new TrackingStream([1, 2, 3, 4, 5], canSeek: false);
        var tools = CreateDownloadTools(stream, 4);

        var exception = await Assert.ThrowsAsync<McpException>(() => tools.GetAttachment("page-1", "a.bin"));

        exception.Message.Should().StartWith("attachment_too_large:");
        stream.IsDisposed.Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Should_ReturnBase64Content_WhenDownloadIsExactlyAtLimit(bool canSeek)
    {
        byte[] bytes = [1, 2, 3, 4];
        using var stream = new TrackingStream(bytes, canSeek);
        var tools = CreateDownloadTools(stream, bytes.Length);

        var json = await tools.GetAttachment("page-1", "a.bin");

        using var document = JsonDocument.Parse(json);
        document.RootElement.GetProperty("content").GetString().Should().Be(Convert.ToBase64String(bytes));
        stream.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldThrow_AttachmentNotFound_WhenDownloadIsMissing()
    {
        var tools = CreateDownloadTools(null, 4);

        var exception = await Assert.ThrowsAsync<McpException>(() => tools.GetAttachment("page-1", "a.bin"));

        exception.Message.Should().StartWith("attachment_not_found:");
    }

    private static AttachmentTools CreateDownloadTools(Stream stream, long maxBytes)
    {
        var attachments = A.Fake<IAttachmentService>();
        A.CallTo(() => attachments.GetAttachmentContent("page-1", "a.bin", A<Mimisbrunnr.Wiki.Contracts.UserInfo>._, maxBytes))
            .Returns(Task.FromResult(stream));
        var context = McpTestFactory.CreateContext();
        return new AttachmentTools(attachments, new McpServerConfiguration { MaxAttachmentBytes = maxBytes },
            context, McpTestFactory.CreateExecutor(context));
    }

    private sealed class TrackingStream(byte[] bytes, bool canSeek = true) : MemoryStream(bytes)
    {
        public override bool CanSeek => canSeek;
        public int ReadCalls { get; private set; }
        public bool IsDisposed { get; private set; }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            ReadCalls++;
            return base.ReadAsync(buffer, cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
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
