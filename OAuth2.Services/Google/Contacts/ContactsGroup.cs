using System;
using System.Collections;
using System.Collections.Generic;
using OfficeClip.OpenSource.OAuth2.Lib;
using System.Web.Script.Serialization;

namespace OfficeClip.OpenSource.OAuth2.Services.Google.Contacts
{
    public class ContactsGroup
    {
        private const string endpointUrl = "https://www.google.com/m8/feeds/groups/default/full";
        public const string ScopeReadOnly = "https://www.googleapis.com/auth/contacts.readonly";
        public const string ScopeReadWrite = "https://www.google.com/m8/feeds/";
        public List<Entry> Entries;
        private readonly HttpAuthResponse response;

        public ContactsGroup(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new Exception("Access Token is required to get user info");
            }
            var parameters = new Dictionary<string, string> {
                { "alt", "json" }
            };
            response = Utils.MakeWebRequest(endpointUrl, parameters, false, accessToken);
            if (response != null)
            {
                var serializer = new JavaScriptSerializer();
                var contactFeed = serializer.Deserialize<Dictionary<string, object>>(ToJsonString());
                var feed = (Dictionary < string, object>)contactFeed["feed"];
                var entry = (ArrayList)feed["entry"];
                Entries = new List<Entry>();
                foreach (Dictionary<string, object> entryItem in entry)
                {
                    var id = (Dictionary<string, object>)entryItem["id"];
                    var title = (Dictionary <string, object>) entryItem["title"];
                    var group = new Entry
                                  {
                                      Id = (string) id["$t"],
                                      Name = (string) title["$t"]
                                  };
                    Entries.Add(group);
                }
            }
        }
        public string ToJsonString()
        {
            return response.ResponseString ?? string.Empty;
        }

        public class Entry
        {
            public string Id;
            public string Name;
        }

    }
}
