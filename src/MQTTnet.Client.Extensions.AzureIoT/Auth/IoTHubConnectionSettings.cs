using System;
using System.Collections.Generic;
using System.Text;

namespace MQTTnet.Client.Extensions.AzureIoT.Auth
{

    public class IoTHubConnectionSettings : ConnectionSettings
    {
        private const int Default_SasMinutes = 60;
        
        public string IdScope { get; set; }
        
        public string DeviceId { get; set; }
        public string ModuleId { get; set; }

        public string SharedAccessKey { get; set; }
        
        //public AuthType Auth
        //{
        //    get => !string.IsNullOrEmpty(X509Key) ? AuthType.X509 :
        //            !string.IsNullOrEmpty(SharedAccessKey) ? AuthType.Sas :
        //                AuthType.Basic;
        //}
        public int SasMinutes { get; set; }
        
        public string GatewayHostName { get; set; }

        public string ModelId { get; set; }

        public IoTHubConnectionSettings()
        {
            SasMinutes = Default_SasMinutes;
        }

        public static new IoTHubConnectionSettings FromConnectionString(string cs) => new IoTHubConnectionSettings(cs);
        public IoTHubConnectionSettings(string cs) => ParseConnectionString(cs);

        private static string GetStringValue(IDictionary<string, string> dict, string propertyName, string defaultValue = "")
        {
            string result = defaultValue;
            if (dict.TryGetValue(propertyName, out string value))
            {
                result = value;
            }
            return result;
        }

        private static int GetPositiveIntValueOrDefault(IDictionary<string, string> dict, string propertyName, int defaultValue)
        {
            int result = defaultValue;
            if (dict.TryGetValue(propertyName, out string stringValue))
            {
                if (int.TryParse(stringValue, out int intValue))
                {
                    result = intValue;
                }
            }
            return result;
        }

        private new void ParseConnectionString(string cs) 
        {
            base.ParseConnectionString(cs);
            IDictionary<string, string> map = cs.ToDictionary(';', '=');
            IdScope = GetStringValue(map, nameof(IdScope));
            DeviceId = GetStringValue(map, nameof(DeviceId));
            SharedAccessKey = GetStringValue(map, nameof(SharedAccessKey));
            ModuleId = GetStringValue(map, nameof(ModuleId));
            ModelId = GetStringValue(map, nameof(ModelId));
            SasMinutes = GetPositiveIntValueOrDefault(map, nameof(SasMinutes), Default_SasMinutes);
            GatewayHostName = GetStringValue(map, nameof(GatewayHostName));
        }

        private static void AppendIfNotEmpty(StringBuilder sb, string name, string val)
        {
            if (!string.IsNullOrEmpty(val))
            {
                if (name.Contains("Key"))
                {
                    sb.Append($"{name}=***;");
                }
                else
                {
                    sb.Append($"{name}={val};");
                }
            }
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            AppendIfNotEmpty(result, nameof(HostName), HostName);
            AppendIfNotEmpty(result, nameof(TcpPort), TcpPort.ToString());
            AppendIfNotEmpty(result, nameof(DeviceId), DeviceId);
            AppendIfNotEmpty(result, nameof(IdScope), IdScope);
            AppendIfNotEmpty(result, nameof(ModuleId), ModuleId);
            AppendIfNotEmpty(result, nameof(SharedAccessKey), SharedAccessKey);
            AppendIfNotEmpty(result, nameof(UserName), UserName);
            AppendIfNotEmpty(result, nameof(X509Key), X509Key);
            AppendIfNotEmpty(result, nameof(ModelId), ModelId);
            AppendIfNotEmpty(result, nameof(ClientId), ClientId);
            AppendIfNotEmpty(result, nameof(MqttVersion), MqttVersion.ToString());
            AppendIfNotEmpty(result, nameof(GatewayHostName), GatewayHostName);
            result.Remove(result.Length - 1, 1);
            return result.ToString();
        }
    }
}