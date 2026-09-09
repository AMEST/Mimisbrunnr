using FluentAssertions;
using Mimisbrunnr.DataImport;

namespace Mimisbrunnr.Web.Tests.Host;

public class ConfluenceContentProcessingTests
{
    [Fact]
    public void Should_ReplacePagePlaceholder_WhenPostProcessingAttachmentLinks()
    {
        var result = ConfluenceContentProcessing.PostProcess(
            "![image](/api/attachment/%%pageId%%/file.png)", "page-42");

        result.Should().Contain("/api/attachment/page-42/file.png");
    }

    [Fact]
    public void Should_HtmlEncodeCode_WhenProcessingCodeMacro()
    {
        var content = "<ac:structured-macro ac:name=\"code\"><ac:plain-text-body><![CDATA[<script>alert(1)</script>]]></ac:plain-text-body></ac:structured-macro>";

        var result = ConfluenceContentProcessing.Process(content);

        result.Should().Contain("&lt;script&gt;");
        result.Should().NotContain("<script>alert");
    }

    [Fact]
    public void Should_CreateDownloadLink_WhenProcessingNonImageAttachment()
    {
        var result = ConfluenceContentProcessing.Process("<ri:attachment ri:filename=\"manual.pdf\" />");

        result.Should().Contain("<a target=\"_blank\"")
            .And.Contain("manual.pdf");
    }
}
