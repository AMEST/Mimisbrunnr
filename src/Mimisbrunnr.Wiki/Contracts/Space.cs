using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Wiki.Contracts;

public class Space : IHasId<string>
{
    public string Id { get; set; }
    
    public string Key { get; internal set; }
    
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    public string HomePageId { get; internal set; }
    
    public SpaceType Type { get; set; }
    
    public SpaceStatus Status { get; internal set; }

    public IEnumerable<Permission> Permissions { get; internal set; }
}