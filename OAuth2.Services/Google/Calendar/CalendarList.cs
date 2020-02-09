using System;
using System.Collections.Generic;
using OfficeClip.OpenSource.OAuth2.Lib;
using System.Web.Script.Serialization;

namespace OfficeClip.OpenSource.OAuth2.Services.Google.Calendar
{
    public class CalendarList
    {
        private const string endpointUrl = "https://www.googleapis.com/calendar/v3/users/me/calendarList";
        public const string ScopeReadOnly = "https://www.googleapis.com/auth/calendar.readonly";
        public const string ScopeReadWrite = "https://www.googleapis.com/auth/calendar";

        private readonly HttpAuthResponse response;
        public List<CalendarItem> CalendarItems { get; set; }
        public CalendarList(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new Exception("Access Token is required to get user info");
            }
            response = Utils.MakeWebRequest(endpointUrl, null, false, accessToken);
            if (response != null)
            {
                var serializer = new JavaScriptSerializer();
                var calendarItems = serializer.Deserialize<CalendarItems>(ToJsonString());
                CalendarItems = calendarItems.Items;
            }
        }
        public string ToJsonString()
        {
            return response.ResponseString ?? string.Empty;
        }
    }

    public class CalendarItems
    {
        public List<CalendarItem> Items;
    }

    public class CalendarItem
    {
        public string Id;
        public string Summary;
    }
}
