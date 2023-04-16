using MQTTnet.Extensions.ManagedClient;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Client.Extensions.AzureIoT
{
    public class IotHubDeviceClient
    {
        private readonly string _connectionString;
        private readonly IMqttClient _mqttClient;

        private GetTwinBinder _getTwinBinder;
        private UpdateTwinBinder<object> _updateTwinBinder;
        private DesiredUpdatePropertyBinder _desiredUpdateBinder;
        private Command _commandBinder;

        public IotHubDeviceClient(IMqttClient mqttClient)
        {
            _mqttClient = mqttClient;
            _getTwinBinder = new GetTwinBinder(_mqttClient);
            _commandBinder = new Command(_mqttClient);
            _updateTwinBinder = new UpdateTwinBinder<object>(_mqttClient);
            _desiredUpdateBinder = new DesiredUpdatePropertyBinder(_mqttClient, _updateTwinBinder);
        }

        public IotHubDeviceClient(IManagedMqttClient mqttClient)
        {
            _mqttClient = mqttClient.InternalClient;
            _getTwinBinder = new GetTwinBinder(_mqttClient);
            _commandBinder = new Command(_mqttClient);
            _updateTwinBinder = new UpdateTwinBinder<object>(_mqttClient);
            _desiredUpdateBinder = new DesiredUpdatePropertyBinder(_mqttClient, _updateTwinBinder);
        }

        public IotHubDeviceClient(string connectionString)
        {
            _connectionString = connectionString;
            _mqttClient = new MqttFactory().CreateMqttClient(MqttNetTraceLogger.CreateTraceLogger());
        }

        public async Task OpenAsync(CancellationToken ct = default)
        {
            var cs = new ConnectionSettings(_connectionString);
            await _mqttClient.ConnectAsync(
                new MqttClientOptionsBuilder()
                .WithTcpServer(cs.HostName, cs.TcpPort)
                .WithCredentials(new SasCredentials(cs.HostName, cs.DeviceId, cs.SharedAccessKey))
                .WithClientId(cs.DeviceId)
                .WithTls(new MqttClientOptionsBuilderTlsParameters()
                {
                    UseTls = true
                })
                .Build(), ct);

            _getTwinBinder = new GetTwinBinder(_mqttClient);
            _commandBinder = new Command(_mqttClient);
            _updateTwinBinder = new UpdateTwinBinder<object>(_mqttClient);
            _desiredUpdateBinder = new DesiredUpdatePropertyBinder(_mqttClient, _updateTwinBinder);
        }

        public async Task SetDirectMethodCallbackAsync(Func<DirectMethodRequest, Task<DirectMethodResponse>> userCallback)
        {
            _commandBinder.OnCmdDelegate = async req =>
            {
                var dmResp = await userCallback.Invoke(new DirectMethodRequest() { MethodName = req.CommandName, JsonPayload = req.CommandPayload });
                CommandResponse resp = new CommandResponse()
                {
                    ReponsePayload = dmResp.Payload,
                    Status = dmResp.Status,
                };
                return await Task.FromResult(resp);
            };
            await Task.Yield();
        }

        public async Task SetDesiredPropertyUpdateCallbackAsync(Func<DesiredProperties, Task> userCallback)
        {
            _desiredUpdateBinder.OnProperty_Updated = desired =>
            {
                
                userCallback.Invoke(new DesiredProperties(desired));
                return new PropertyAck() { Value = desired.ToJsonString() };
            };
            await Task.Yield();
        }

        public async Task<TwinProperties> GetTwinPropertiesAsync(CancellationToken stoppingToken = default)
        {
            var twin = await _getTwinBinder.InvokeAsync(_mqttClient.Options.ClientId, string.Empty, stoppingToken);
            var twinProps = new TwinProperties(twin);
            return twinProps;
        }

        public async Task SendTelemetryAsync(TelemetryMessage telemetryMessage, CancellationToken t = default)
        {
            await _mqttClient.PublishBinaryAsync($"devices/{_mqttClient.Options.ClientId}/messages/events/",
              new Utf8JsonSerializer().ToBytes(telemetryMessage.Payload),
              Protocol.MqttQualityOfServiceLevel.AtLeastOnce,
              false, t);
        }

        public async Task<long> UpdateReportedPropertiesAsync(ReportedProperties reportedProperties, CancellationToken ct = default)
        {
            var twin = await _updateTwinBinder.InvokeAsync(_mqttClient.Options.ClientId, reportedProperties, ct);
            return twin;
        }
    }
}
