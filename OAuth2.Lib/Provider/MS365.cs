using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace OfficeClip.OpenSource.OAuth2.Lib.Provider
{
    public class MS365UserInfo
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("givenName")]
        public string GivenName { get; set; }

        [JsonProperty("surName")]
        public string SurName { get; set; }

        [JsonProperty("picture")]
        public string Picture { get; set; }

        [JsonProperty("userPrincipalName")]
        public string UserPrincipalName { get; set; }
    }

    public class MS365 : Client
    {
        private const string MSGraphScope = "openid email user.read";
        private const string ExchangeScope = "offline_access https://outlook.office.com/IMAP.AccessAsUser.All https://outlook.office.com/SMTP.Send";
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

        public void SetGraphToken()
        {
            _scope = MSGraphScope;
        }

        public void SetExchangeToken()
        {
            _scope = ExchangeScope;
        }

        public override UserInfo GetUserInfo()
        {
            if (string.IsNullOrWhiteSpace(AccessToken))
            {
                throw new Exception("Access Token is required to get user info");
            }

            //var response = Utils.MakeWebRequest(UserInfoUrl, null, false, AccessToken);
            return GetUserInfoFromIdToken();
        }

        /// <summary>
        /// Extract the limited user information from the id token
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private UserInfo GetUserInfoFromIdToken()
        {
            UserInfo userInfo;
            var token = new JwtSecurityToken(jwtEncodedString: IdToken);
            if (token == null)
            {
                throw new Exception("User Information could not be retrived from the Id Token");
            }
            try
            {
                userInfo = new UserInfo
                {
                    Id = token.Claims.First(c => c.Type == "sid").Value
                };

                userInfo.FirstName =
                            token.Claims.Where(c => c.Type == "first_name").ToList().Count == 0
                            ? string.Empty
                            : token.Claims.First(c => c.Type == "given_name").Value;
                userInfo.LastName =
                            token.Claims.Where(c => c.Type == "family_name").ToList().Count == 0
                            ? string.Empty
                            : token.Claims.First(c => c.Type == "family_name").Value;

                userInfo.Email = token.Claims.First(c => c.Type == "email").Value;
                userInfo.FullName = $"{userInfo.FirstName} {userInfo.LastName}";
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to parse JSON response: {ex.Message}");
            }
            return userInfo;
        }

        public HttpAuthResponse GetSharedSecretAccessToken()
        {
            var parameters = new Dictionary<string, string>
                                                        {
                                                            {Key.ClientId, ClientId},
                                                            {Key.ClientSecret, ClientSecret},
                                                            {Key.Scope, "https://graph.microsoft.com/.default" },
                                                            { Key.GrantType, "client_credential"}
                                                        };
            var response = Utils.MakeWebRequest(
                                    "https://login.microsoftonline.com/common/oauth2/v2.0/token",
                                    parameters,
                                    true
                                    );
            return response;
        }
    }
}
