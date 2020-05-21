﻿using System.Configuration;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace ResiliencePatternsDotNet.ConsoleApplication.Configurations
{
    [XmlRoot("configuration")]
    public class ConfigurationSection : IConfigurationSectionHandler
    {
        public static ConfigurationSection Instance
            => (ConfigurationSection) ConfigurationManager.GetSection("configuration");
        
        [XmlAttribute("request-configuration")]
        public RequestConfigurationSection RequestConfiguration { get; set; }
        
        [XmlAttribute("url-configuration")]
        public UrlConfigurationSection UrlConfiguration { get; set; }

        public object Create(object parent, object configContext, XmlNode section)
        {
            var ser = new XmlSerializer(typeof(ConfigurationSection));
            using (var sr = new StringReader(section.OuterXml))
                return (ConfigurationSection) ser.Deserialize(sr);
        }
    }
}