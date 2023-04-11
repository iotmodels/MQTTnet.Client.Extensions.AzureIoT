using System.Text.Json.Nodes;
using System.Text.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet.Extensions.ManagedClient;

namespace MQTTnet.Client.Extensions.AzureIoT
{
    public class HubDeviceClient
    {
        private readonly IMqttClient _mqttClient;

        private readonly GetTwinBinder getTwinBinder;
        private readonly UpdateTwinBinder<object> updateTwinBinder;

        private readonly GenericDesiredUpdatePropertyBinder genericDesiredUpdateProperty;
        private readonly Command command;

        public static async Task<HubDeviceClient> CreateClientAsync(string hostname, string deviceId, string sasKey)
        {
            var client = new MqttFactory().CreateMqttClient(MqttNetTraceLogger.CreateTraceLogger());
            await client.ConnectAsync(
                new MqttClientOptionsBuilder()
                .WithTcpServer(hostname, 8883)
                .WithCredentials(new SasCredentials(hostname, deviceId, sasKey))
                .WithClientId(deviceId)
                .WithTls(new MqttClientOptionsBuilderTlsParameters()
                {
                    UseTls = true
                })
                .Build());
            return new HubDeviceClient(client);
        }

        public static async Task<HubDeviceClient> CreateManagedClientAsync(string hostname, string deviceId, string sasKey, TimeSpan retryDelay)
        {
            var client = new MqttFactory().CreateManagedMqttClient(MqttNetTraceLogger.CreateTraceLogger());
            await client.StartAsync(
                new ManagedMqttClientOptionsBuilder()
                .WithClientOptions(
                    new MqttClientOptionsBuilder()
                    .WithTcpServer(hostname, 8883)
                    .WithCredentials(new SasCredentials(hostname, deviceId, sasKey))
                    .WithClientId(deviceId)
                    .WithTls(new MqttClientOptionsBuilderTlsParameters()
                    {
                        UseTls = true
                    })
                    .Build())
                .WithAutoReconnectDelay(retryDelay)
                .Build());
            return new HubDeviceClient(client.InternalClient);
        }


        public HubDeviceClient(IMqttClient mqttClient)
        {
            _mqttClient = mqttClient;

            getTwinBinder = new GetTwinBinder(mqttClient);
            updateTwinBinder = new UpdateTwinBinder<object>(mqttClient);
            command = new Command(mqttClient);
            genericDesiredUpdateProperty = new GenericDesiredUpdatePropertyBinder(mqttClient, updateTwinBinder);
        }

        public Func<CommandRequest, Task<CommandResponse>> OnCommandReceived
        {
            get => command.OnCmdDelegate;
            set => command.OnCmdDelegate = value;
        }

        public Func<JsonNode, GenericPropertyAck> OnPropertyUpdateReceived
        {
            get => genericDesiredUpdateProperty.OnProperty_Updated;
            set => genericDesiredUpdateProperty.OnProperty_Updated = value;
        }

        public async Task<string> GetTwinAsync(CancellationToken cancellationToken = default)
        {
            var twin = await getTwinBinder.InvokeAsync(_mqttClient.Options.ClientId, string.Empty, cancellationToken);
            return twin.ToString();
        }

        public async Task<int> UpdateTwinAsync(object payload, CancellationToken cancellationToken = default)
        {
            var twin = await updateTwinBinder.InvokeAsync(_mqttClient.Options.ClientId, JsonSerializer.Serialize(payload), cancellationToken);
            return twin;
        }

        public Task<MqttClientPublishResult> SendTelemetryAsync(object payload, CancellationToken t = default) => 
            _mqttClient.PublishBinaryAsync($"devices/{_mqttClient.Options.ClientId}/messages/events/",
                new Utf8JsonSerializer().ToBytes(payload),
                Protocol.MqttQualityOfServiceLevel.AtLeastOnce,
                false, t);
    }
}
