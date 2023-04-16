using memmon.device;
using MQTTnet;
using MQTTnet.Client;

namespace memmon.sdklite
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddHostedService<Device>();
                })
                .Build();

            host.Run();
        }

        public static async Task<IotHubDeviceClient> CreateFromConnectionStringAsync0(string connectionString, ILogger logger) 
        {
            var client = new IotHubDeviceClient(connectionString)
            {
                ConnectionStatusChangeCallback = c => logger.LogWarning("Connection status changed: {s}", c.Status)
            };
            await client.OpenAsync();
            return client;
        }

        public static async Task<IotHubDeviceClient> CreateFromConnectionStringAsync(string connectionString, ILogger logger)
        {
            var mqttClient = new MqttFactory().CreateMqttClient(MqttNetTraceLogger.CreateTraceLogger());
            await mqttClient.ConnectAsync(new MqttClientOptionsBuilder().WithConnectionSettings(new ConnectionSettings(connectionString)).Build());
            var client = new IotHubDeviceClient(mqttClient)
            {
                ConnectionStatusChangeCallback = c => logger.LogWarning("Connection status changed: {s}", c.Status)
            };
            return client;
        }
    }
}