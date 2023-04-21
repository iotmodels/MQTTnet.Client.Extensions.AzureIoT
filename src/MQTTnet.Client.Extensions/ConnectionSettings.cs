﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MQTTnet.Client.Extensions
{
    public class ConnectionSettings
    {
        private const int Default_KeepAliveInSeconds = 60;
        private const string Default_CleanSession = "true";
        private const int Default_TcpPort = 8883;
        private const string Default_UseTls = "true";
        private const string Default_DisableCrl = "false";
        private const int Default_MqttVersion = 5;

        public string HostName { get; set; }
        public string ClientId { get; set; }
        public string X509Key { get; set; } //paht-to.pfx|pfxpwd, or thumbprint
        
        public string UserName { get; set; }
        public string Password { get; set; }
        public int KeepAliveInSeconds { get; set; }
        public bool CleanSession { get; set; }
        public int TcpPort { get; set; }
        public bool UseTls { get; set; }
        public string CaFile { get; set; }
        public bool DisableCrl { get; set; }

        public int? MqttVersion { get; set; }

        public ConnectionSettings()
        {
            TcpPort = Default_TcpPort;
            KeepAliveInSeconds = Default_KeepAliveInSeconds;
            UseTls = Default_UseTls == "true";
            DisableCrl = Default_DisableCrl == "true";
            CleanSession = Default_CleanSession == "true";
            MqttVersion = Default_MqttVersion;
        }

        public static ConnectionSettings FromConnectionString(string cs) => new ConnectionSettings(cs);
        public ConnectionSettings(string cs) => ParseConnectionString(cs);

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

        protected void ParseConnectionString(string cs)
        {
            IDictionary<string, string> map = cs.ToDictionary(';', '=');
            HostName = GetStringValue(map, nameof(HostName));
            ClientId = GetStringValue(map, nameof(ClientId));
            X509Key = GetStringValue(map, nameof(X509Key));
            UserName = GetStringValue(map, nameof(UserName));
            Password = GetStringValue(map, nameof(Password));
            KeepAliveInSeconds = GetPositiveIntValueOrDefault(map, nameof(KeepAliveInSeconds), Default_KeepAliveInSeconds);
            CleanSession = GetStringValue(map, nameof(CleanSession), Default_CleanSession) == "true";
            TcpPort = GetPositiveIntValueOrDefault(map, nameof(TcpPort), Default_TcpPort);
            UseTls = GetStringValue(map, nameof(UseTls), Default_UseTls) == "true";
            CaFile = GetStringValue(map, nameof(CaFile));
            DisableCrl = GetStringValue(map, nameof(DisableCrl), Default_DisableCrl) == "true";
            MqttVersion = GetPositiveIntValueOrDefault(map, nameof(MqttVersion), Default_MqttVersion);
            if (Validate( out string msg) == false)
            {
                throw new FormatException($"Invalid ConnectionSettings: {msg}");
            }
        }

        private bool Validate(out string validationMessage)
        {
            validationMessage = string.Empty;

            if (MqttVersion != 3 && MqttVersion != 5)
            {
                validationMessage = $"Mqtt Version {MqttVersion} not supported, should be '3' or '5'";
                return false;
            }

            if (string.IsNullOrEmpty(HostName))
            {
                validationMessage = "HostName is mandatory";
                return false;
            }

            return true;
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
            AppendIfNotEmpty(result, nameof(UserName), UserName);
            AppendIfNotEmpty(result, nameof(X509Key), X509Key);
            AppendIfNotEmpty(result, nameof(ClientId), ClientId);
            AppendIfNotEmpty(result, nameof(MqttVersion), MqttVersion.ToString());
            result.Remove(result.Length - 1, 1);
            return result.ToString();
        }
    }
}