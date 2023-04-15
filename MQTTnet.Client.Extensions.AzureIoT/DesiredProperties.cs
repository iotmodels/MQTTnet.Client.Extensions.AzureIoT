using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace MQTTnet.Client.Extensions.AzureIoT
{
    public class DesiredProperties
    {
        private readonly Dictionary<string, object> _properties;
        public long Version { get; set; }

        public DesiredProperties()
        {
            _properties = new Dictionary<string, object>();
        }
        public string GetSerializedString()
        {
            return JsonSerializer.Serialize(_properties);
        }

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
    }
}
