using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MQTTnet.Client.Extensions.AzureIoT
{
    public class ReportedProperties
    {
        public Dictionary<string, object> _properties { get; set; }
        public ReportedProperties()
        {
            _properties = new Dictionary<string, object>();
        }

        public ReportedProperties(Dictionary<string, object> props, bool responseFromService = false)
        {
            _properties = props;
        }

        [JsonPropertyName("$version")]
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