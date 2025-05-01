using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Wiki.Contracts;

public class Plugin : IHasId<string>
{
    public string Id { get; set; }
    public string PluginIdentifier { get; set; }
    public string Name { get; set; }
    public string Version { get; set; }
    public UserInfo InstalledBy { get; internal set; }
    public DateTime Installation { get; internal set; }
    public bool Disabled { get; internal set; }
    public Macro[] Macros { get; set; } = [];
}