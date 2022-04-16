using System.Text.Json.Serialization;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Wiki.Contracts;

public class Space : IHasId<string>
{

    public Space()
    {
    }

    [JsonConstructor]
    public Space(string id, string key, string name, string description, string homePageId, SpaceType type, SpaceStatus status, IEnumerable<Permission> permissions)
    {
        HomePageId = homePageId;
        Key = key;
        Id = id;
        Name = name;
        Description = description;
        Type = type;
        Status = status;
        Permissions = permissions;
    }
    public string Id { get; set; }

    public string Key { get; internal set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string HomePageId { get; internal set; }

    public SpaceType Type { get; set; }

    public SpaceStatus Status { get; internal set; }

    public IEnumerable<Permission> Permissions { get; internal set; }
}