using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Wiki.Contracts;

public class Attachment : IHasId<string>
{
    public string Id { get; set; }

    public string PageId { get; set; }

    public string Path { get; set; }

    public string Name { get; set; }

    public DateTime Created { get; set; }

    public UserInfo CreatedBy { get; set; }
}