﻿using System.Configuration;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace ResiliencePatternsDotNet.ConsoleApplication.Configurations
{
    [XmlRoot("circuit-breaker-configuration")]
    public class CircuitBreakerConfiguration : IConfigurationSectionHandler
    {
        
        public object Create(object parent, object configContext, XmlNode section)
        {
            var ser = new XmlSerializer(typeof(CircuitBreakerConfiguration));
            using (var sr = new StringReader(section.OuterXml))
                return (CircuitBreakerConfiguration) ser.Deserialize(sr);
        }
    }
}