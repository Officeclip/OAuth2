using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace OfficeClip.OpenSource.OAuth2.Lib.Provider
{
    public class GoogleUserInfo
    {
        /// <summary>
        /// ID issued by provider.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// E-mail for user.
        /// </summary>
        public string Email { get; set; }

        public string Verified_Email { get; set; }

        /// <summary>
        /// First name of user.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// First name of user.
        /// </summary>
        public string Given_Name { get; set; }

        /// <summary>
        /// Last name of user.
        /// </summary>
        public string Family_Name { get; set; }

        /// <summary>
        /// Time-zone of user.
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// Username of user.
        /// </summary>
        public string Picture { get; set; }
        /// <summary>
        /// Gender of user.
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// Locale of user.
        /// </summary>
        public string Locale { get; set; }

        /// <summary>
        /// Domain of the login user
        /// </summary>
        public string Hd { get; set; }

    }

    public class Google : Client
    {
        public bool ForceRefreshToken { get; set; }
        public string Domain { get; set; }
        public Google(
            string clientId,
            string clientSecret,
            string scope,
            string redirectUri) : base(clientId, clientSecret, scope, redirectUri)
        {
            AuthorizationUrl = "https://accounts.google.com/o/oauth2/auth";
            AccessTokenUrl = "https://accounts.google.com/o/oauth2/token";
            RefreshTokenUrl = "https://accounts.google.com/o/oauth2/token";
            ValidateTokenUrl = "https://www.googleapis.com/oauth2/v1/tokeninfo";
            UserInfoUrl = "https://www.googleapis.com/oauth2/v1/userinfo";
            IsValidateAccessToken = true;
        }

        public override void BeforeAuthorizationCodeRequest(Dictionary<string, string> parameters)
        {
            if (ForceRefreshToken)
            {
                parameters.Add(
                    Key.AccessType, "offline");
                parameters.Add(
                    Key.ApprovalPrompt, "force");
            }
            if (!string.IsNullOrEmpty(Domain))
            {
                parameters.Add(Key.Domain, Domain);
            }
        }

        protected override UserInfo ExtractUserInfo(HttpAuthResponse response)
        {
            UserInfo returnValue = null;
            if (response == null)
                return null;
            try
            {
                if (Utils.IsJson(response.ResponseString))
                {
                    throw new Exception($"String not in json format: {response.ResponseString}");
                }
                var googleUserInfo = new JavaScriptSerializer().Deserialize<GoogleUserInfo>(response.ResponseString);
                returnValue = new UserInfo
                {
                    Id = googleUserInfo.Id,
                    FullName = googleUserInfo.Name,
                    FirstName = googleUserInfo.Given_Name,
                    LastName = googleUserInfo.Family_Name,
                    Email = googleUserInfo.Email,
                    Gender = googleUserInfo.Gender,
                    Locale = googleUserInfo.Locale,
                    SocialLink = googleUserInfo.Link,
                    PictureUrl = googleUserInfo.Picture,
                    Domain = googleUserInfo.Hd
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to parse JSON response: {ex.Message}");
            }
            return returnValue;
        }
    }
}