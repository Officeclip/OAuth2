using System;
using System.Collections.Generic;
using OfficeClip.OpenSource.OAuth2.Lib;
using System.Web.Script.Serialization;


namespace OfficeClip.OpenSource.OAuth2.Services.Google.Directory
{
    public class DomainUsers
    {
        public List<DomainUser> Users { get; set; }

        private const string endpointUrl = "https://www.googleapis.com/admin/directory/v1/users";
        public const string ScopeReadOnly = "https://www.googleapis.com/auth/admin.directory.user.readonly";
        public const string ScopeReadWrite = "https://www.googleapis.com/auth/admin.directory.user";
        private readonly HttpAuthResponse response;
        public DomainUsers(string accessToken, Dictionary<string, string> parameters = null)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new Exception("Access Token is required to get user info");
            }
            if (parameters == null) {
                parameters = new Dictionary<string, string> {
                                            { "domain", "officeclip.com" }
                };
            }
            response = Utils.MakeWebRequest(endpointUrl, parameters, false, accessToken);
            if (response != null)
            {
                var serializer = new JavaScriptSerializer();
                var users = serializer.Deserialize<AdminDirectoryUsers>(ToJsonString());
                Users = users.Users;
            }
        }

        public string ToJsonString()
        {
            return response.ResponseString ?? string.Empty;
        }

        public DomainUser GetUserByEmail(string emailAddress)
        {
            foreach (var googleDomainUser in Users)
            {
                if (googleDomainUser.PrimaryEmail == emailAddress)
                {
                    return googleDomainUser;
                }
            }
            return null;
        }
    }

    public class AdminDirectoryUsers
    {
        public List<DomainUser> Users;
    }
    public class DomainUser
    {
        public DomainUserName Name;
        public string PrimaryEmail;
        public bool IsAdmin;
        public string ThumbNailPhotoUrl;
        public string Id;
    }

    public class DomainUserName
    {
        public string GivenName;
        public string FamilyName;
    }
}
