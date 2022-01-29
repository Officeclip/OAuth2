using OfficeClip.OpenSource.OAuth2.Lib;
using System;
using System.Collections.Generic;
using System.Net;

namespace OfficeClip.OpenSource.OAuth2.Services.Slack
{
    public class Messaging
    {
        private const string endpointUrl = "https://slack.com/api/chat.postMessage";
        private HttpAuthResponse response;

        public void Post(string accessToken, string channel, string blocks)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new Exception("Access Token is required to get user info");
            }
            var parameters = new Dictionary<string, string>();
            parameters.Add("channel", channel);
            parameters.Add("blocks", blocks);
            parameters.Add("token", accessToken);
            response = Utils.MakeWebRequest(endpointUrl, parameters, true, accessToken);
        }
    }
}
