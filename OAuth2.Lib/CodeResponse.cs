using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfficeClip.OpenSource.OAuth2.Lib
{
    public class CodeResponse
    {
        /// <summary>
        /// Code from provider.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Token issued by the provider.
        /// </summary>
        public string Access_Token { get; set; }

        /// <summary>
        /// Token issued by the provider.
        /// </summary>
        public string Refresh_Token { get; set; }

        /// <summary>
        /// Amount of second til token expires.
        /// </summary>
        public int Expires_In { get; set; }

        /// <summary>
        /// The type of token issued by the provider.
        /// </summary>
        public string Token_Type { get; set; }
    }
}
