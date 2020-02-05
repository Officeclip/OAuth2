using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace OfficeClip.OpenSource.OAuth2.Lib.Configuration
{
    public class Element : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name => base["name"].ToString();

        [ConfigurationProperty("clientId", IsRequired = true)]
        public string ClientId => base["clientId"].ToString();

        [ConfigurationProperty("clientSecret", IsRequired = true)]
        public string ClientSecret => base["clientSecret"].ToString();

        [ConfigurationProperty("scope", IsRequired = false)]
        public string Scope => base["scope"].ToString();

        [ConfigurationProperty("redirectUri", IsRequired = false)]
        public string RedirectUri => base["redirectUri"].ToString();
    }

}
