using Mimisbrunnr.Integration.User;

namespace Mimisbrunnr.Integration.Plugin;

public class MacroRenderRequest
{
    public string PluginIdentifier { get; set; }

    public string MacroIdentifier { get; set; }

    public UserModel RequestedBy { get; set; }

    public string PageId { get; set; }

    public string SpaceKey { get; set; }

    public string? UserToken { get; set; } 
    
    public IDictionary<string, string> Params { get; set; }
}