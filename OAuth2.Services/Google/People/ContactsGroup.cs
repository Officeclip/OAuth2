using System;
using System.Collections;
using System.Collections.Generic;
using OfficeClip.OpenSource.OAuth2.Lib;
using System.Web.Script.Serialization;

namespace OfficeClip.OpenSource.OAuth2.Services.Google.People
{
    public class ContactsGroup
    {
        private const string endpointUrl = "https://people.googleapis.com/v1/contactGroups";
        private readonly HttpAuthResponse response;
        private string _accessToken;

        public ContactsGroup(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new Exception("Access Token is required to get user info");
            }
            _accessToken = accessToken;
        }

        public List<Tuple<string, string>> Get()
        {
            var entries = new List<Tuple<string, string>>();
            var parameters = new Dictionary<string, string> {
                { "alt", "json" }
            };
            var response = Utils.MakeWebRequest(endpointUrl, parameters, false, _accessToken);
            if (response != null)
            {
                var serializer = new JavaScriptSerializer();
                var contactFeed = serializer.Deserialize<Dictionary<string, object>>(ToJsonString());
                var feed = (Dictionary<string, object>)contactFeed["feed"];
                var entry = (ArrayList)feed["entry"];
                foreach (Dictionary<string, object> entryItem in entry)
                {
                    var id = (Dictionary<string, object>)entryItem["id"];
                    var title = (Dictionary<string, object>)entryItem["title"];
                    var group = new Tuple<string, string>((string)id["$t"], (string)title["$t"]);
                    entries.Add(group);
                }
            }
            return entries;
        }

        private string ToJsonString()
        {
            return response.ResponseString ?? string.Empty;
        }

    }
}
