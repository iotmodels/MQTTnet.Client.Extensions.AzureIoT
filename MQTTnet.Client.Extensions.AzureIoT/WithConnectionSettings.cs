using System;
using System.Collections.Generic;
using System.Text;

namespace MQTTnet.Client.Extensions.AzureIoT
{
    public static partial class MqttNetExtensions
    {
        public static MqttClientOptionsBuilder WithConnectionSettings(this MqttClientOptionsBuilder builder, ConnectionSettings cs)
        {
            builder
                .WithTcpServer(cs.HostName, cs.TcpPort)
                .WithCredentials(new SasCredentials(cs.HostName, cs.DeviceId, cs.SharedAccessKey))
                .WithClientId(cs.DeviceId)
                .WithTls(new MqttClientOptionsBuilderTlsParameters()
                {
                    UseTls = true
                });
            return builder;
        }
    }
}
