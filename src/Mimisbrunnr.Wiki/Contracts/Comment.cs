using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Wiki.Contracts;

public class Comment : IHasId<string>
{
    public string Id { get; set; }

    public string PageId { get; set; }

    public string Message { get; set; }

    public DateTime Created { get; set; }

    public UserInfo Author { get; set; }
}