namespace Mimisbrunnr.Integration.Plugin;
public class MacroRenderUserRequest
{
    public string? MacroIdentifier { get; set; }
    public IDictionary<string, string> Params { get; set; } = new Dictionary<string, string>();
}