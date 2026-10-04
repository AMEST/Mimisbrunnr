namespace Mimisbrunnr.Web.Customization
{
    /// <summary>
    /// Model representing the custom homepage configuration
    /// </summary>
    public class CustomHomepageModel
    {
        /// <summary>
        /// Key of the space whose home page is used as the custom homepage
        /// </summary>
        public string HomepageSpaceKey { get; set; }

        /// <summary>
        /// Identifier of the page used as the custom homepage
        /// </summary>
        public string HomepageId { get; set; }
    }
}