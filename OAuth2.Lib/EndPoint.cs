using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfficeClip.OpenSource.OAuth2.Lib
{
    public class Endpoint
    {
        /// <summary>
        /// URL for user-redirection to provider auth-page.
        /// </summary>
        public string AuthorizationUrl { get; set; }

        /// <summary>
        /// URL for access token validation.
        /// </summary>
        public string AccessTokenUrl { get; set; }

        public string ValidateTokenUrl { get; set; }

        /// <summary>
        /// URL for access token refresh.
        /// </summary>
        public string RefreshTokenUrl { get; set; }

        /// <summary>
        /// URL for user infomation gathering.
        /// </summary>
        public string UserInfoUrl { get; set; }
    }

}
