using System;
using System.Collections.Generic;

namespace OfficeClip.OpenSource.OAuth2.Lib
{
    public interface IClient
    {
        string ClientId { get; }
        string ClientSecret { get; }
        string Scope { get; }
        string RedirectUri { get; }
        string AccessToken { get; }
        string RefreshToken { get; }
        DateTime AccessTokenExpiration { get; }
        //State StateObject { get; }
        bool IsValidateAccessToken { get; }
        void BeforeAuthorizationCodeRequest(Dictionary<string, string> parameters);
        void BeforeAccessCodeAuthorizationCodeRequest(Dictionary<string, string> parameters);
    }
}
