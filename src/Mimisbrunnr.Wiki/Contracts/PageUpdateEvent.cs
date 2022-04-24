using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Wiki.Contracts;

public class PageUpdateEvent : IHasId<string>
{
    public string Id { get; set; }

    public string SpaceKey { get; set; }

    public SpaceType SpaceType { get; set; }

    public string PageTitle { get; set; }

    public DateTime Updated { get; set; }

    public UserInfo UpdatedBy { get; set; }
}