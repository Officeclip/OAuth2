using OfficeClip.OpenSource.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace OfficeClip.OpenSource.OAuth2.Lib
{
    public abstract class Client : Endpoint, IClient
    {
         public string ClientId { get; private set; }
        public string ClientSecret { get; private set; }
        public string Scope { get; private set; }
        public string RedirectUri { get; private set; }
        public string AccessToken { get; private set; }
        public string RefreshToken { get; set; }
        public DateTime AccessTokenExpiration { get; private set; }
        public string TokenType { get; private set; }
        public bool IsAuthorized { get; private set; }
        public bool IsValidateAccessToken { get; set; }

        /// <summary>
        /// The current request object to use throughout the library.
        /// </summary>
        private HttpRequest Request
        {
            get
            {
                return HttpContext.Current.Request;
            }
        }

        /// <summary>
        /// The current response object to use throughout the library.
        /// </summary>
        private HttpResponse Response
        {
            get
            {
                return HttpContext.Current.Response;
            }
        }

        public virtual void BeforeAuthorizationCodeRequest(Dictionary<string, string> parameters)
        {
        }

        public virtual void BeforeAccessCodeAuthorizationCodeRequest(Dictionary<string, string> parameters)
        {
        }

        protected virtual UserInfo ExtractUserInfo(HttpAuthResponse response)
        {
            return null;
        }

        protected Client(
            string clientId,
            string clientSecret,
            string scope,
            string redirectUri)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            Scope = scope;
            RedirectUri = redirectUri;
        }

        public void Authenticate(State stateObject = null)
        {
            GetAccessTokenFromRefreshToken();
            if (string.IsNullOrWhiteSpace(AccessToken) || !IsAccessTokenValid())
            {
                AuthorizationCodeRequest(stateObject);
            }
        }

        private void AuthorizationCodeRequest(State stateObject = null, string nonceString = "")
        {
            stateObject = stateObject ?? new State(nonceString);
            Dictionary<string, string> parameters = new Dictionary<string, string> {
                { Key.ClientId, ClientId },
                { Key.RedirectUri, RedirectUri },
                { Key.Scope, Scope },
                { Key.ResponseType, Value.Code },
                { Key.State, stateObject.ToString() }
            };
            BeforeAuthorizationCodeRequest(parameters);
            string url = $"{AuthorizationUrl}?{Utils.BuildQueryString(parameters)}";
            try
            {
                Response.Redirect(url, true);
            }
            catch (ThreadAbortException ex1)
            {

            }
            catch (Exception ex)
            {
                throw new Exception(
                    "OpenNetTools.OAuth2.Lib.Client.Authorization Code Request Failed",
                    ex);
            }
        }

        public void GetAccessTokenFromRefreshToken()
        {
            if (!string.IsNullOrWhiteSpace(RefreshToken))
            {
                var parameters = new Dictionary<string, string>
                                                        {
                                                            {Key.RefreshToken, RefreshToken},
                                                            {Key.ClientId, ClientId},
                                                            {Key.ClientSecret, ClientSecret},
                                                            {Key.GrantType, Value.RefreshToken}
                                                        };

                var response = Utils.MakeWebRequest(RefreshTokenUrl, parameters, true);
                if (response.ResponseString != null)
                {
                    AnalyzeAccessTokenResponse(response.ResponseString);
                }
            }
        }

        /// <summary>
        /// Check for OAuth2 code response and attempt to validate it.
        /// </summary>
        public void HandleAuthorizationCodeResponse()
        {
            var code = Request.QueryString["code"];
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new Exception("Authorization code should be present in the response");
            }

            var error = Request.QueryString["error"];
            if (!string.IsNullOrWhiteSpace(error))
            {
                throw new Exception($"Authorization code response returned an error: {error}");
            }

            var response = AccessTokenFromAuthorizationCodeRequest(code);

            AnalyzeAccessTokenResponse(response.ResponseString);
        }

        public State GetStateObject(string nonceString)
        {
            var stateString = Request.QueryString["state"];
            State stateObject = new State(stateString, nonceString);
            if (!stateObject.IsValid)
            {
                throw new Exception("The state string that was sent did not match to the value received. Suspected Hijack!");
            }
            return stateObject;
        }

        private HttpAuthResponse AccessTokenFromAuthorizationCodeRequest(string code)
        {
            var parameters = new Dictionary<string, string> {
                {Key.ClientId, ClientId},
                {Key.ClientSecret, ClientSecret},
                {Key.RedirectUri, RedirectUri},
                {Key.Code, code},
                {Key.GrantType, Value.AuthorizationCode}
            };
            BeforeAccessCodeAuthorizationCodeRequest(parameters);
            return Utils.MakeWebRequest(AccessTokenUrl, parameters, true);
        }

        private bool IsAccessTokenValid()
        {
            if (!IsValidateAccessToken)
            {
                return true;
            }
            if (AccessToken != null)
            {
                var parameters = new Dictionary<string, string> {
                    {Key.AccessToken, AccessToken}
                };
                var response = Utils.MakeWebRequest(ValidateTokenUrl, parameters, false);
                IsAccessTokenValidResponse(response.ResponseString);
                return true;
            }
            return false;
        }

        private void IsAccessTokenValidResponse(string response)
        {
            if (response?.IndexOf("Error") > 0)
            {
                throw new Exception("The access token could not be validated by the server");
            }
        }


        /// <summary>
        /// Attempt to analyze access-token response, either in string or JSON format.
        /// </summary>
        /// <param name="response">Strong or JSON response.</param>
        private void AnalyzeAccessTokenResponse(string response)
        {
            if (response == null)
                return;

            AccessToken = null;
            AccessTokenExpiration = DateTime.MinValue;

            if (Utils.IsJson(response))
            {
                try
                {
                    var codeResponse = new JavaScriptSerializer().Deserialize<CodeResponse>(response);

                    if (!string.IsNullOrWhiteSpace(codeResponse.Access_Token))
                        AccessToken = codeResponse.Access_Token;

                    if (!string.IsNullOrWhiteSpace(codeResponse.Refresh_Token))
                        RefreshToken = codeResponse.Refresh_Token;

                    if (codeResponse.Expires_In > 0)
                        AccessTokenExpiration = DateTime.Now.AddSeconds(codeResponse.Expires_In);

                    if (!string.IsNullOrWhiteSpace(codeResponse.Token_Type))
                        TokenType = codeResponse.Token_Type;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Unable to parse JSON response: {ex.Message}");
                }
            }
            else
            {
                foreach (var entry in response.Split('&'))
                {
                    if (entry.IndexOf('=') == -1)
                        continue;

                    var key = entry.Substring(0, entry.IndexOf('='));
                    var value = entry.Substring(entry.IndexOf('=') + 1);

                    switch (key)
                    {
                        case Key.AccessToken:
                            AccessToken = value;
                            break;
                        case Key.RefreshToken:
                            RefreshToken = value;
                            break;
                        case Key.Expires:
                        case Key.ExpiresIn:
                            int exp;
                            if (int.TryParse(value, out exp))
                                AccessTokenExpiration = DateTime.Now.AddSeconds(exp);
                            break;
                        case Key.TokenType:
                            TokenType = value;
                            break;
                    }
                }
            }
            if (IsAccessTokenValid())
            {
                IsAuthorized = (!string.IsNullOrWhiteSpace(AccessToken) &&
                                     (AccessTokenExpiration > DateTime.Now));
            }
            else
            {
                IsAuthorized = false;
            }
        }

        public UserInfo GetUserInfo()
        {
            if (string.IsNullOrWhiteSpace(AccessToken))
            {
                throw new Exception("Access Token is required to get user info");
            }
            var parameters = new Dictionary<string, string> {
                { Key.AccessToken, AccessToken },
                { "alt", "json" }
            };

            var response = Utils.MakeWebRequest(UserInfoUrl, parameters, false);
            return ExtractUserInfo(response);
        }
    }
}
