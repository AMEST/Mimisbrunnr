using Mimisbrunnr.Wiki.Contracts;
using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.PageTemplates.Contracts;

public class PageTemplate : IHasId<string>
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Content { get; set; }
    public string Type { get; set; }
    public string OwnerEmail { get; set; }
    public string SpaceId { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
    public UserInfo CreatedBy { get; set; }
    public UserInfo UpdatedBy { get; set; }
}
