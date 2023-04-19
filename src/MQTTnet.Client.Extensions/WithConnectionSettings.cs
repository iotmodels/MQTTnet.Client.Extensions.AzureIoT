using System;
using System.Collections.Generic;
using System.Text;

namespace MQTTnet.Client.Extensions
{
    public static partial class MqttNetExtensions
    {
        public static MqttClientOptionsBuilder WithConnectionSettings(this MqttClientOptionsBuilder builder, ConnectionSettings cs)
        {
            builder
                .WithTcpServer(cs.HostName, cs.TcpPort)
                .WithCredentials(new SasCredentials(cs.HostName, cs.DeviceId, cs.SharedAccessKey, cs.SasMinutes, cs.ModelId))
                .WithClientId(cs.DeviceId)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(cs.KeepAliveInSeconds))
                .WithTls(new MqttClientOptionsBuilderTlsParameters()
                {
                    UseTls = cs.UseTls
                });
            return builder;
        }
    }
}
