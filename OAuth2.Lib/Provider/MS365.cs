using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfficeClip.OpenSource.OAuth2.Lib.Provider
{
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
            UserInfoUrl = "";
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
    }
}
