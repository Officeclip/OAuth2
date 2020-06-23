using OfficeClip.OpenSource.OAuth2.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OfficeClip.OpenSource.OAuth2.Services.Slack
{
    public class ChannelList
    {
        private const string endpointUrl = "https://slack.com/api/conversations.list";

        private HttpAuthResponse response;

        public List<Channel> GetChannelList(string accessToken)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            List<Channel> retChannels = new List<Channel>();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new Exception("Access Token is required to get user info");
            }
            response = Utils.MakeWebRequest(endpointUrl, null, false, accessToken);
            if (response != null)
            {
                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                var slackResponse = serializer.Deserialize<SlackMessageResponse>(ToJsonString());
                retChannels = slackResponse.channels;
            }
            return retChannels;
        }

        public string ToJsonString()
        {
            return response.ResponseString ?? string.Empty;
        }
    }

    

    public class Channel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class SlackMessageResponse
    {
        public bool ok { get; set; }
        public String error { get; set; }
        public List<Channel> channels { get; set; }
        public String ts { get; set; }
    }
}
