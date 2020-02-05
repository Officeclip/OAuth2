using System.Configuration;

namespace OfficeClip.OpenSource.OAuth2.Lib.Configuration
{
    public class ElementCollection :ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new Element();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var authConfigurationElement = element as Element;
            if (authConfigurationElement != null) return authConfigurationElement.Name;
            return "";
        }

    }
}
