namespace OfficeClip.OpenSource.OAuth2.Lib
{
    public class UserInfo
    {
        /// <summary>
        /// ID issued by provider.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// E-mail for user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// First name of user.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Last name of user.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Full name of user.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gender of user.
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// Locale of user.
        /// </summary>
        public string Locale { get; set; }

        public string PictureUrl { get; set; }

        public string SocialLink { get; set; }
        /// <summary>
        /// The domain that the user is registered in, useful for google apps login
        /// </summary>
        public string Domain { get; set; }
    }
}
