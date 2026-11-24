using System.Text.Json.Serialization;

namespace Mimisbrunnr.Integration.Plugin;

public class MacroRenderResponse
{
    public string Html { get; set; }

    /// <summary>
    /// Parameters resolved from page content, saved macro state and plugin defaults.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IDictionary<string, string> Params { get; set; }
}