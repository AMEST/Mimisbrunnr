using System.Text.RegularExpressions;
using System.Web;

namespace Mimisbrunnr.DataImport;

public static class ConfluenceContentProcessing
{
    private static readonly Regex FindImageAcRegEx = new("<ac:image\\s[^>]*><ri:url\\sri:value=\"([^\"]*)\"\\s\\/><\\/ac:image>", RegexOptions.Compiled);
    private static readonly Regex FindCodeMacroRegEx = new(
        "<ac:structured-macro\\sac:name=\"code\"[^>]*>[\\s\\n]*(?:<ac:parameter[^>]*>[^>]*>)*<ac:plain-text-body><!\\[CDATA\\[([\\s\\S]*?)\\]{2}\\s*\\]?><\\/ac:plain-text-body><\\/ac:structured-macro>"
        , RegexOptions.Compiled);
    private static readonly Regex FindAttachmentMacroRegEx = new("<ri:attachment\\sri:filename=\"([^\"]*)\"\\s\\/>", RegexOptions.Compiled);
    private static readonly Regex FindKDrawIOMacroRegEx = new("<ac:structured-macro\\sac:name=\"kontur-draw-io\"[^>]*>.*(?=<\\/ac:structured-macro>)<\\/ac:structured-macro>", RegexOptions.Compiled);
    private static readonly Regex FindDrawIOMacroRegEx = new("<ac:structured-macro\\sac:name=\"drawio\"[^>]*>[\\s\\n]*(?:<ac:parameter[^>]*>[^>]*>)*<\\/ac:structured-macro>", RegexOptions.Compiled);
    private static readonly Regex FindDrawIOMacroSubRegEx = new("ac:name=\"diagramName\"\\>([^<]*)<\\/ac:parameter>", RegexOptions.Compiled);

    public static string Process(string content)
    {
        var processingContent = content;

        processingContent = ProcessImages(processingContent);
        processingContent = ProcessCodeMacro(processingContent);
        processingContent = ProcessAttachments(processingContent);
        processingContent = ProcessDrawMacro(processingContent, FindKDrawIOMacroRegEx, "svg");
        processingContent = ProcessDrawMacro(processingContent, FindDrawIOMacroRegEx, "png");

        return processingContent;
    }

    public static string PostProcess(string content, string pageId)
    {
        return content.Replace("/api/attachment/%%pageId%%/", $"/api/attachment/{pageId}/");
    }

    private static string ProcessImages(string processingContent)
    {
        var imageDetect = FindImageAcRegEx.Matches(processingContent);
        if (!imageDetect.Any())
            return processingContent;

        foreach (Match match in imageDetect)
        {
            var foundedString = match.Value;
            var imageSrc = match.Groups.Values.LastOrDefault()?.Value;
            processingContent = processingContent.Replace(foundedString, $"<img src=\"{imageSrc}\"/>");
        }
        return processingContent;
    }

    private static string ProcessCodeMacro(string processingContent)
    {
        var codeDetect = FindCodeMacroRegEx.Matches(processingContent);
        if (!codeDetect.Any())
            return processingContent;

        foreach (Match match in codeDetect)
        {
            var foundedString = match.Value;
            var code = match.Groups.Values.LastOrDefault()?.Value;
            processingContent = processingContent.Replace(foundedString, $"<pre><code>{HttpUtility.HtmlEncode(code)}</code></pre>");
        }
        return processingContent;
    }

    private static string ProcessAttachments(string processingContent)
    {
        var attachments = FindAttachmentMacroRegEx.Matches(processingContent);
        if (!attachments.Any())
            return processingContent;

        foreach (Match match in attachments)
        {
            var foundedString = match.Value;
            var filename = match.Groups.Values.LastOrDefault()?.Value;
            if (filename.ToLower().EndsWith(".png") ||
                filename.ToLower().EndsWith(".jpg") ||
                filename.ToLower().EndsWith(".jpeg") ||
                filename.ToLower().EndsWith(".gif") ||
                filename.ToLower().EndsWith(".svg"))
            {
                processingContent = processingContent.Replace(foundedString, $"<img src=\"/api/attachment/%%pageId%%/{filename.UrlEncode()}\"/>");
            }
            else
            {
                processingContent = processingContent.Replace(foundedString, $"<a target=\"_blank\" href=\"/api/attachment/%%pageId%%/{filename.UrlEncode()}\"/>");
            }
        }
        return processingContent;
    }

    private static string ProcessDrawMacro(string processingContent, Regex findRegEx, string diagramPreviewExtension)
    {
        var macro = findRegEx.Matches(processingContent);
        if (!macro.Any())
            return processingContent;

        foreach (Match match in macro)
        {
            var foundedString = match.Value;
            var diagramNameSearch = FindDrawIOMacroSubRegEx.Match(foundedString);
            var diagramName = diagramNameSearch.Groups.Values.LastOrDefault()?.Value;
            processingContent = processingContent.Replace(foundedString, $"<img src=\"/api/attachment/%%pageId%%/{diagramName}.{diagramPreviewExtension}\"/>");
        }
        return processingContent;
    }

    private static string UrlEncode(this string text)
    {
        return HttpUtility.UrlEncode(text).Replace("+", "%20");
    }
}
