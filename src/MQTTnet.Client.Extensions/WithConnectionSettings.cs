using System;

namespace MQTTnet.Client.Extensions
{
    public static partial class MqttNetExtensions
    {
        public static MqttClientOptionsBuilder WithConnectionSettings(this MqttClientOptionsBuilder builder, ConnectionSettings cs)
        {
            builder
                .WithTcpServer(cs.HostName, cs.TcpPort)
                .WithCredentials(cs.UserName, cs.Password)
                .WithClientId(cs.ClientId)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(cs.KeepAliveInSeconds))
                .WithCleanSession(cs.CleanSession)
                .WithTls(new MqttClientOptionsBuilderTlsParameters()    
                {
                    UseTls = cs.UseTls
                });
            return builder;
        }
    }
}
