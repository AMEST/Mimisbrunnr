using Skidbladnir.Repository.Abstractions;

namespace Mimisbrunnr.Wiki.Contracts
{
    public class HistoricalPage : IHasId<string>
    {
        public string Id { get; set; }
        public string PageId { get; set; }
        public long Version { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public DateTime Updated { get; internal set; }
        public UserInfo UpdatedBy { get; internal set; }

        public static HistoricalPage Create(Page page)
        {
            return new HistoricalPage(){
                PageId = page.Id,
                Version = page.Version,
                Name = page.Name,
                Content = page.Content,
                Updated = page.Updated,
                UpdatedBy = page.UpdatedBy
            };
        }
    }
}