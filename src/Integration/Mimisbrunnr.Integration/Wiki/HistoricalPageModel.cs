using Mimisbrunnr.Integration.User;

namespace Mimisbrunnr.Integration.Wiki;

public class HistoricalPageModel
{
    public string Id { get; set; }

    public string PageId { get; set; }

    public long Version { get; set; }

    public string Name { get; set; }

    public string Content { get; set; }

    public DateTime Created { get; set; }

    public DateTime Updated { get; set; }

}