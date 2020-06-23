using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfficeClip.OpenSource.OAuth2.Lib.Provider
{
    public class Slack : Client
    {
        public Slack(
            string clientId,
            string clientSecret,
            string scope,
            string redirectUri) : base(clientId, clientSecret, scope, redirectUri)
        {

            AuthorizationUrl = "https://slack.com/oauth/v2/authorize";
            AccessTokenUrl = "https://slack.com/api/oauth.v2.access";
            ChannelListUrl = "https://slack.com/api/conversations.list";
            ChatUrl = "https://slack.com/api/chat.postMessage";
        }
    }
}
