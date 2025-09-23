namespace Mimisbrunnr.Integration.Wiki
{
    /// <summary>
    /// Model representing a list of page versions
    /// </summary>
    public class PageVersionsListModel
    {
        /// <summary>
        /// Array of historical page versions
        /// </summary>
        public HistoricalPageModel[] Versions { get; set; }

        /// <summary>
        /// Latest version number
        /// </summary>
        public long LatestVersion { get; set; }

        /// <summary>
        /// Date of the latest version
        /// </summary>
        public DateTime LatestVersionDate { get; set; }

        /// <summary>
        /// Total count of versions
        /// </summary>
        public long Count { get; set; }
    }
}
