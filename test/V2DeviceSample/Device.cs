//using Microsoft.Azure.Devices.Client;

using MQTTnet.Client.Extensions.AzureIoT;
using MQTTnet.Extensions.ManagedClient;

namespace V2DeviceSample
{
    public class Device : BackgroundService
    {
        private readonly ILogger<Device> _logger;
        private readonly IConfiguration _configuration;
        public Device(ILogger<Device> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string connectionString = _configuration.GetConnectionString("cs")!;

            //IotHubDeviceClient deviceClient = await ConnectAsync(connectionString, stoppingToken);
            //IotHubDeviceClient deviceClient = await ConnectManagedClientAsync(connectionString);

            var deviceClient = new IotHubDeviceClient(connectionString);
            await deviceClient.OpenAsync(stoppingToken);

            await deviceClient.SetDirectMethodCallbackAsync(async m =>
            {
                _logger.LogInformation("Cmd received: {c} with payload {p}", m.MethodName, m.GetPayloadAsJsonString());
                return await Task.FromResult(new DirectMethodResponse(200) { Payload = "ok response" });
            });

            await deviceClient.SetDesiredPropertyUpdateCallbackAsync(async m =>
            {
                _logger.LogInformation("prop received {p}", m.GetSerializedString());
                await Task.Yield();
            });

            var twin = await deviceClient.GetTwinPropertiesAsync(stoppingToken);
            _logger.LogInformation("twin reported: {r}, desired: {d}", twin.Reported.Version, twin.Desired.Version);
            _logger.LogInformation("twin reported: {r}, desired: {d}", twin.Reported.GetSerializedString(), twin.Desired.GetSerializedString());

            var reportedProperties = new ReportedProperties();
            reportedProperties["started"] = Environment.WorkingSet;
            var v = await deviceClient.UpdateReportedPropertiesAsync(reportedProperties, stoppingToken);
            _logger.LogInformation("updated started to: {v}", v);

            twin = await deviceClient.GetTwinPropertiesAsync(stoppingToken);
            _logger.LogInformation("twin reported: {r}, desired: {d}", twin.Reported.Version, twin.Desired.Version);
            _logger.LogInformation("twin reported: {r}, desired: {d}", twin.Reported.GetSerializedString(), twin.Desired.GetSerializedString());

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Sending Telemetry: {time}", DateTimeOffset.Now);
                await deviceClient.SendTelemetryAsync(new TelemetryMessage(new { Environment.WorkingSet }), stoppingToken);
                await Task.Delay(5000, stoppingToken);
            }
        }

        private static async Task<IotHubDeviceClient> ConnectManagedClientAsync(string connectionString)
        {
            var mqttClient = new MQTTnet.MqttFactory().CreateManagedMqttClient(MqttNetTraceLogger.CreateTraceLogger());
            await mqttClient.StartAsync(new ManagedMqttClientOptionsBuilder()
                    .WithClientOptions(new MQTTnet.Client.MqttClientOptionsBuilder()
                    .WithConnectionSettings(new ConnectionSettings(connectionString))
                    .Build())
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .Build());
            while (mqttClient.IsConnected == false) { await Task.Delay(100); }
            var deviceClient = new IotHubDeviceClient(mqttClient);
            return deviceClient;
        }

        private static async Task<IotHubDeviceClient> ConnectAsync(string connectionString, CancellationToken stoppingToken)
        {
            var mqttClient = new MQTTnet.MqttFactory().CreateMqttClient(MqttNetTraceLogger.CreateTraceLogger());
            await mqttClient.ConnectAsync(new MQTTnet.Client.MqttClientOptionsBuilder()
                .WithConnectionSettings(new ConnectionSettings(connectionString))
                .Build(), stoppingToken);
            var deviceClient = new IotHubDeviceClient(mqttClient);
            return deviceClient;
        }
    }
}