using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

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

        public DesiredProperties(JsonNode node)
        {
            _properties = new Dictionary<string, object>();

            // TODO: review
            var parser = JsonDocument.Parse(node.ToJsonString());

            foreach (var el in parser.RootElement.EnumerateObject())
            {
                if (el.Name == "$version")
                {
                    Version = el.Value.GetInt64();
                }
                else
                {
                    switch (el.Value.ValueKind)
                    {
                        case JsonValueKind.String:
                            _properties.Add(el.Name, el.Value.GetString());
                            break;
                        case JsonValueKind.Number:
                            if (el.Value.TryGetInt32(out int i))
                            {
                                _properties.Add(el.Name, i);
                                break;
                            }
                            else if (el.Value.TryGetInt64(out long l))
                            {
                                _properties.Add(el.Name, l);
                                break;
                            }
                            else if (el.Value.TryGetDouble(out double d))
                            {
                                _properties.Add(el.Name, d);
                                break;
                            }
                            break;

                        default:
                            _properties.Add(el.Name, el.Value);
                            break;
                    };
                }
            }
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
