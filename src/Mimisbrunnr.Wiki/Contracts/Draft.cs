using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Wiki.Contracts;

public class Draft : IHasId<string>
{
    public string Id { get; set; }

    public string OriginalPageId { get; set; }

    public string Name { get; set; }

    public string Content { get; set; }

    public DateTime Updated { get; internal set; }
    
    public UserInfo UpdatedBy { get; internal set; }
}