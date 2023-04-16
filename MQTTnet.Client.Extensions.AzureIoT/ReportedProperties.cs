using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;

namespace MQTTnet.Client.Extensions.AzureIoT
{
    public class ReportedProperties 
    {
        private readonly Dictionary<string, object> _properties;
        public ReportedProperties()
        {
            _properties = new Dictionary<string, object>();
        }

        public ReportedProperties(Dictionary<string, object> props, bool responseFromService = false)
        {
            _properties = props;
        }

        public long Version { get; set; }

        public object this[string propertyKey]
        {
            get
            {
                return _properties[propertyKey];
            }
            set
            {
                _properties.Add(propertyKey, value);
            }
        }

        public string GetSerializedString()
        {
            return JsonSerializer.Serialize(_properties);
        }

       
    }
}