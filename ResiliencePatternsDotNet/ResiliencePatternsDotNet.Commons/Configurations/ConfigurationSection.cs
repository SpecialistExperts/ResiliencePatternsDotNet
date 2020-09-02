﻿using System.Configuration;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using ResiliencePatternsDotNet.Commons.Common;
using ResiliencePatternsDotNet.Commons.Configurations.CircuitBreakerConfigurations;

namespace ResiliencePatternsDotNet.Commons.Configurations
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
        
        [XmlAttribute("prometheus-configuration")]
        public PrometheusConfiguration PrometheusConfiguration { get; set; }
        
        [XmlAttribute("run-policy")]
        public RunPolicyEnum RunPolicy { get; set; }
        
        [XmlAttribute("retry-configuration")]
        public RetryConfiguration RetryConfiguration { get; set; }
        
        [XmlAttribute("circuit-breaker-configuration")]
        public CircuitBreakerConfiguration CircuitBreakerConfiguration { get; set; }

        public object Create(object parent, object configContext, XmlNode section)
        {
            var ser = new XmlSerializer(typeof(ConfigurationSection));
            using (var sr = new StringReader(section.OuterXml))
                return (ConfigurationSection) ser.Deserialize(sr);
        }
    }
}