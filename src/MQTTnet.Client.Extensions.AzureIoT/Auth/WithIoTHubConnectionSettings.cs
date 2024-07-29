using System;

namespace MQTTnet.Client.Extensions.AzureIoT.Auth
{
    public static partial class MqttNetExtensions
    {
        public static MqttClientOptionsBuilder WithIoTHubConnectionSettings(this MqttClientOptionsBuilder builder, IoTHubConnectionSettings cs)
        {
            builder
                .WithTcpServer(cs.HostName, cs.TcpPort)
                .WithCredentials(new SasCredentials(cs.HostName, cs.DeviceId, cs.SharedAccessKey, cs.SasMinutes, cs.ModelId))
                .WithClientId(cs.DeviceId)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(cs.KeepAliveInSeconds))
                .WithTlsOptions(new MqttClientTlsOptionsBuilder().Build());
            return builder;
        }
    }
}
