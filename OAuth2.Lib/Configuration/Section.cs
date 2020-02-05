using OfficeClip.OpenSource.OAuth2.Lib.Configuration;
using System.Configuration;

namespace OfficeClip.OpenSource.Lib.Configuration
{
    public class Section : ConfigurationSection
    {
        [ConfigurationProperty("provider", IsKey = false, IsRequired = true)]
        [ConfigurationCollection(typeof(ElementCollection),
            CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
        public ElementCollection OAuthVClientConfigurations
        {
            get { return base["provider"] as ElementCollection; }
        }
    }
}
