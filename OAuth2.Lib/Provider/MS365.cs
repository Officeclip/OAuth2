using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace OfficeClip.OpenSource.OAuth2.Lib.Provider
{
    public class MS365UserInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Given_Name { get; set; }
        public string Family_Name { get; set; }
        public string Picture { get; set; }
        public string Email { get; set; }
    }

    public class MS365 : Client
    {
        private string _scope;
        public MS365(
            string clientId,
            string clientSecret,
            string scope,
            string redirectUri,
            string tenantId
            ) : base(clientId, clientSecret, scope, redirectUri)
        {
            _scope = scope;
            AuthorizationUrl = $"https://login.microsoftonline.com/common/oauth2/v2.0/authorize";
            AccessTokenUrl = $"https://login.microsoftonline.com/common/oauth2/v2.0/token";
            RefreshTokenUrl = $"https://login.microsoftonline.com/common/oauth2/v2.0/token";
            ValidateTokenUrl = "";
            UserInfoUrl = "https://graph.microsoft.com/v1.0/me";
            IsValidateAccessToken = false;
        }

        public override void BeforeAuthorizationCodeRequest(Dictionary<string, string> parameters)
        {
        }

        public override void BeforeAccessCodeAuthorizationCodeRequest(Dictionary<string, string> parameters)
        {
            parameters.Add(
                Key.Scope, _scope);
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
                var MS365UserInfo = new JavaScriptSerializer().Deserialize<MS365UserInfo>(response.ResponseString);
                returnValue = new UserInfo
                {
                    Id = MS365UserInfo.Id,
                    FullName = MS365UserInfo.Name,
                    FirstName = MS365UserInfo.Given_Name,
                    LastName = MS365UserInfo.Family_Name,
                    Email = MS365UserInfo.Email,
                    //Gender = MS365UserInfo.Gender,
                    //SocialLink = MS365UserInfo.Link,
                    PictureUrl = MS365UserInfo.Picture
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
