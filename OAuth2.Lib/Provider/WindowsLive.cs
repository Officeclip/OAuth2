using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace OfficeClip.OpenSource.OAuth2.Lib.Provider
{
    public class WindowsLiveInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string First_name { get; set; }
        public string Last_name { get; set; }
        public string Link { get; set; }
        public string Gender { get; set; }
        public WindowsLiveEmailInfo Emails { get; set; }
    }

    public class WindowsLiveEmailInfo
    {
        public string Preferred { get; set; }
        public string Account { get; set; }
        public string Personal { get; set; }
        public string Business { get; set; }
    }

    public class WindowsLive : Client
    {
        public bool ForceRefreshToken { get; set; }

        public WindowsLive(
            string clientId,
            string clientSecret,
            string scope,
            string redirectUri) : base(clientId, clientSecret, scope, redirectUri)
        {
            AuthorizationUrl = "https://login.live.com/oauth20_authorize.srf";
            AccessTokenUrl = "https://login.live.com/oauth20_token.srf";
            RefreshTokenUrl = "https://login.live.com/oauth20_token.srf";
            ValidateTokenUrl = "https://login.live.com/oauth20_token.srf";
            UserInfoUrl = "https://apis.live.net/v5.0/me";
            IsValidateAccessToken = false;
        }

        public override void BeforeAuthorizationCodeRequest(Dictionary<string, string> parameters)
        {
            parameters.Add(
                Key.Display, Value.Popup);
        }

        public override void BeforeAccessCodeAuthorizationCodeRequest(Dictionary<string, string> parameters)
        {

        }

        protected override UserInfo ExtractUserInfo(HttpAuthResponse response)
        {
            UserInfo returnValue = null;
            if (response == null)
                return null;
            try
            {
                if (!Utils.IsJson(response.ResponseString))
                {
                    throw new Exception($"String not in json format: {response.ResponseString}");
                }
                var windowsLiveUserInfo = new JavaScriptSerializer().Deserialize<WindowsLiveInfo>(response.ResponseString);
                returnValue = new UserInfo
                {
                    Id = windowsLiveUserInfo.Id,
                    FullName = windowsLiveUserInfo.Name,
                    FirstName = windowsLiveUserInfo.First_name,
                    LastName = windowsLiveUserInfo.Last_name,
                    Email = windowsLiveUserInfo.Emails.Account,
                    Gender = windowsLiveUserInfo.Gender,
                    SocialLink = windowsLiveUserInfo.Link,
                    PictureUrl = $"https://apis.live.net/v5.0/{windowsLiveUserInfo.Id}/picture"
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