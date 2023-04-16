using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

namespace memmon.sdklite
{
    internal static class ClientFactory
    {
        public static async Task<IotHubDeviceClient> CreateFromConnectionStringAsync0(string connectionString, ILogger logger)
        {
            var client = new IotHubDeviceClient(connectionString)
            {
                ConnectionStatusChangeCallback = c => logger.LogWarning("Connection status changed: {s}", c.Status)
            };
            await client.OpenAsync();
            return client;
        }

        public static async Task<IotHubDeviceClient> CreateFromConnectionStringAsync1(string connectionString, ILogger logger)
        {
            var mqttClient = new MqttFactory().CreateMqttClient(MqttNetTraceLogger.CreateTraceLogger());
            await mqttClient.ConnectAsync(new MqttClientOptionsBuilder().WithConnectionSettings(new ConnectionSettings(connectionString)).Build());
            var client = new IotHubDeviceClient(mqttClient)
            {
                ConnectionStatusChangeCallback = c => logger.LogWarning("Connection status changed: {s}", c.Status)
            };
            return client;
        }


        public static Task<IotHubDeviceClient> CreateFromConnectionStringAsync(string connectionString, ILogger logger)
        {
            var mqttClient = new MqttFactory().CreateManagedMqttClient(MqttNetTraceLogger.CreateTraceLogger());
            mqttClient.StartAsync(new ManagedMqttClientOptionsBuilder()
                .WithClientOptions(new MqttClientOptionsBuilder().WithConnectionSettings(new ConnectionSettings(connectionString)).Build())
                .Build()).Wait();

            var tcs = new TaskCompletionSource<IotHubDeviceClient>();

            mqttClient.ConnectedAsync += async c =>
            {
                var client = new IotHubDeviceClient(mqttClient)
                {
                    ConnectionStatusChangeCallback = c => logger.LogWarning("Connection status changed: {s}", c.Status)
                };
                tcs.TrySetResult(client);
                await Task.Yield();
            };
            return tcs.Task;
        }
    }
}