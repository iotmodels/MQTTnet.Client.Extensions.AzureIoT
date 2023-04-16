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

        public static async Task<IotHubDeviceClient> CreateFromConnectionStringAsync1(string connectionString, ILogger logger, CancellationToken ct = default)
        {
            var mqttClient = new MqttFactory().CreateMqttClient(MqttNetTraceLogger.CreateTraceLogger());
            await mqttClient.ConnectAsync(new MqttClientOptionsBuilder().WithConnectionSettings(new ConnectionSettings(connectionString)).Build());
            var client = new IotHubDeviceClient(mqttClient)
            {
                ConnectionStatusChangeCallback = c => logger.LogWarning("Connection status changed: {s}", c.Status)
            };
            mqttClient.DisconnectedAsync += async d =>
            {
                while (!ct.IsCancellationRequested)
                {
                    await Task.Delay(1000);
                    try
                    {
                        if (!mqttClient.IsConnected)
                        {
                            await mqttClient.ReconnectAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex.Message);
                    }
                }    
            };
            return client;
        }


        public static Task<IotHubDeviceClient> CreateFromConnectionStringAsync(string connectionString, ILogger logger)
        {
            var mqttClient = new MqttFactory().CreateManagedMqttClient(MqttNetTraceLogger.CreateTraceLogger());
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
            var cs = new ConnectionSettings(connectionString);
            mqttClient.StartAsync(new ManagedMqttClientOptionsBuilder()
                .WithClientOptions(new MqttClientOptionsBuilder().WithConnectionSettings(cs).Build())
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .Build());
            return tcs.Task.TimeoutAfter(TimeSpan.FromSeconds(30));
        }
    }
}