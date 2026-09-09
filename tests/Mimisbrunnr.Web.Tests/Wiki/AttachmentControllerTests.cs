using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mimisbrunnr.Web.Wiki;

namespace Mimisbrunnr.Web.Tests.Wiki;

public class AttachmentControllerTests
{
    [Fact]
    public async Task Should_ReturnNotFound_WhenAttachmentContentIsMissing()
    {
        var attachments = A.Fake<IAttachmentService>();
        A.CallTo(() => attachments.GetAttachmentContent("page", "missing.bin", null))
            .Returns(Task.FromResult<Stream>(null));
        var controller = new AttachmentController(attachments);

        var result = await controller.GetContent("page", "missing.bin");

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Should_ReturnEmptyCollection_WhenPageHasNoAttachments()
    {
        var attachments = A.Fake<IAttachmentService>();
        A.CallTo(() => attachments.GetAttachments("page", null))
            .Returns(Task.FromResult<Mimisbrunnr.Integration.Wiki.AttachmentModel[]>(null));
        var result = await new AttachmentController(attachments).GetAll("page");

        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeAssignableTo<Mimisbrunnr.Integration.Wiki.AttachmentModel[]>();
    }

    [Fact]
    public async Task Should_ForceDownloadAndNosniff_WhenGettingHtmlAttachment()
    {
        var attachments = A.Fake<IAttachmentService>();
        A.CallTo(() => attachments.GetAttachmentContent("page", "payload.html", null))
            .Returns(Task.FromResult<Stream>(new MemoryStream("<script>alert(1)</script>"u8.ToArray())));
        var controller = new AttachmentController(attachments)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var result = await controller.GetContent("page", "payload.html");

        var file = result.Should().BeOfType<FileStreamResult>().Subject;
        file.ContentType.Should().Be("application/octet-stream");
        file.FileDownloadName.Should().Be("payload.html");
        controller.Response.Headers["X-Content-Type-Options"].ToString().Should().Be("nosniff");
    }

    [Fact]
    public async Task Should_KeepInlineResponse_WhenGettingPngAttachment()
    {
        var attachments = A.Fake<IAttachmentService>();
        A.CallTo(() => attachments.GetAttachmentContent("page", "image.png", null))
            .Returns(Task.FromResult<Stream>(new MemoryStream([1, 2, 3])));
        var controller = new AttachmentController(attachments)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };

        var result = await controller.GetContent("page", "image.png");

        var file = result.Should().BeOfType<FileStreamResult>().Subject;
        file.FileDownloadName.Should().BeNullOrEmpty();
        file.ContentType.Should().Be("image/png");
    }
}
