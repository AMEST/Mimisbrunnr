namespace Mimisbrunnr.Integration.Wiki
{
    public class PageVersionsListModel
    {
        public HistoricalPageModel[] Versions { get; set; }

        public long LatestVersion { get; set; }

        public DateTime LatestVersionDate { get; set; }

        public long Count { get; set; }
    }
}