using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Wiki.Contracts;

public class Page : IHasId<string>
{
    public string Id { get; set; }

    public string SpaceId { get; set; }

    public string ParentId { get; set; }

    public string Name { get; set; }

    public string Content { get; set; }

    public DateTime Created { get; internal set; }

    public DateTime Updated { get; internal set; }
    
    public UserInfo CreatedBy { get; internal set; }
    
    public UserInfo UpdatedBy { get; internal set; }

    public Page Clone()
    {
        return new Page()
        {
            SpaceId = SpaceId,
            ParentId = ParentId,
            Name = Name,
            Content = Content,
            Created = Created,
            Updated = Updated,
            CreatedBy = CreatedBy,
            UpdatedBy = UpdatedBy
        };
    }
}