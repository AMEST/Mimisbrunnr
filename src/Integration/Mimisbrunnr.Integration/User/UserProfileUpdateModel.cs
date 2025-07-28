namespace Mimisbrunnr.Integration.User
{
    /// <summary>
    /// Represents the data structure for updating user profile information.
    /// This model is used when users modify their profile details in the Mimisbrunnr application.
    /// </summary>
    public class UserProfileUpdateModel
    {
        /// <summary>
        /// The new website URL for the user's profile.
        /// Can be cleared by setting to null or empty string.
        /// </summary>
        public string Website { get; set; }

        /// <summary>
        /// The updated job title or position for the user's profile.
        /// </summary>
        public string Post { get; set; }

        /// <summary>
        /// The updated department or team information for the user's profile.
        /// </summary>
        public string Department { get; set; }

        /// <summary>
        /// The updated organization or company name for the user's profile.
        /// </summary>
        public string Organization { get; set; }

        /// <summary>
        /// The updated geographic location for the user's profile.
        /// Typically includes city and country information.
        /// </summary>
        public string Location { get; set; }
    }
}
