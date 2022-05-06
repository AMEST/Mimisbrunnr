using System.Text.RegularExpressions;

namespace Mimisbrunnr.Web.Host.Services;

internal static class ConfluenceContentPreprocessing
{
    private static readonly Regex FindImageAcRegEx = new Regex("<ac:image\\s[^>]*><ri:url\\sri:value=\"([^\"]*)\"\\s\\/><\\/ac:image>", RegexOptions.Compiled);
    private static readonly Regex FindCodeMacro = new Regex(
        "<ac:structured-macro\\sac:name=\"code\"[^>]*>[\\s\\n]*(?:<ac:parameter[^>]*>[^>]*>)*<ac:plain-text-body><!\\[CDATA\\[([^\\]\\]]*)]{2}\\s*><\\/ac:plain-text-body><\\/ac:structured-macro>"
        , RegexOptions.Compiled);

    public static string Process(string content)
    {
        var processingContent = content;
        
        processingContent = ProcessImages(processingContent);
        processingContent = ProcessCodeMacro(processingContent);

        return processingContent;
    }

    private static string ProcessImages(string processingContent)
    {
        var imageDetect = FindImageAcRegEx.Matches(processingContent);
        if (!imageDetect.Any())
            return processingContent;

        foreach (Match match in imageDetect)
        {
            var foundedString = match.Value;
            var imageSrc = match.Groups.Values.Last()?.Value;
            processingContent = processingContent.Replace(foundedString, $"<img src=\"{imageSrc}\"/>");
        }
        return processingContent;
    }

    private static string ProcessCodeMacro(string processingContent)
    {
        var codeDetect = FindCodeMacro.Matches(processingContent);
        if (!codeDetect.Any())
            return processingContent;

        foreach (Match match in codeDetect)
        {
            var foundedString = match.Value;
            var code = match.Groups.Values.Last()?.Value;
            processingContent = processingContent.Replace(foundedString, $"<pre><code>{code}</code></pre>");
        }
        return processingContent;
    }
}
