using MQTTnet.Client.Extensions.AzureIoT.Binders;
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
        private readonly IManagedMqttClient _managedMqttClient = null;

        private TelemetryBinder _telemetryBinder;
        private GetTwinBinder _getTwinBinder;
        private UpdateTwinBinder<object> _updateTwinBinder;
        private DesiredUpdatePropertyBinder _desiredUpdateBinder;
        private CommandBinder _commandBinder;

        public Action<ConnectionStatusInfo> ConnectionStatusChangeCallback { get; set; }

        public IotHubDeviceClient(IMqttClient mqttClient)
        {
            _mqttClient = mqttClient;
            _telemetryBinder = new TelemetryBinder(_mqttClient);
            _getTwinBinder = new GetTwinBinder(_mqttClient);
            _commandBinder = new CommandBinder(_mqttClient);
            _updateTwinBinder = new UpdateTwinBinder<object>(_mqttClient);
            _desiredUpdateBinder = new DesiredUpdatePropertyBinder(_mqttClient, _updateTwinBinder);
            _mqttClient.ConnectedAsync += async c =>
            {
                ConnectionStatusChangeCallback?.Invoke(new ConnectionStatusInfo()
                {
                    ChangeReason = c.ConnectResult.ResultCode.ToConnectionStatusChangeReason(),
                    Status = c.ConnectResult.ResultCode.ToConnectionStatus()

                }); ;
                await Task.Yield();
            };
            _mqttClient.DisconnectedAsync += async d =>
            {

                ConnectionStatusChangeCallback?.Invoke(new ConnectionStatusInfo()
                {
                    ChangeReason = d.ConnectResult == null ?
                        ConnectionStatusChangeReason.DeviceDisabled :
                        d.ConnectResult.ResultCode.ToConnectionStatusChangeReason(),
                    Status = d.ConnectResult == null ?
                        ConnectionStatus.Disconnected :
                        d.ConnectResult.ResultCode.ToConnectionStatus()
                });
                await Task.Yield();
            };
        }

        public IotHubDeviceClient(IManagedMqttClient mqttClient) : this(mqttClient.InternalClient)
        {
            _managedMqttClient = mqttClient;
            _telemetryBinder = new TelemetryBinder(_managedMqttClient);
        }

        public IotHubDeviceClient(string connectionString) : this(new MqttFactory().CreateMqttClient(MqttNetTraceLogger.CreateTraceLogger()))
        {
            _connectionString = connectionString;
        }

        public async Task OpenAsync(CancellationToken ct = default)
        {
            var cs = new ConnectionSettings(_connectionString);
            await _mqttClient.ConnectAsync(new MqttClientOptionsBuilder().WithConnectionSettings(cs).Build(), ct);
            _telemetryBinder = new TelemetryBinder(_mqttClient);
            _getTwinBinder = new GetTwinBinder(_mqttClient);
            _commandBinder = new CommandBinder(_mqttClient);
            _updateTwinBinder = new UpdateTwinBinder<object>(_mqttClient);
            _desiredUpdateBinder = new DesiredUpdatePropertyBinder(_mqttClient, _updateTwinBinder);
        }

        public async Task SetDirectMethodCallbackAsync(Func<DirectMethodRequest, Task<DirectMethodResponse>> userCallback, CancellationToken ct = default)
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

        public async Task SetDesiredPropertyUpdateCallbackAsync(Func<DesiredProperties, Task> userCallback, CancellationToken ct = default)
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

        public async Task SendTelemetryAsync(TelemetryMessage telemetryMessage, CancellationToken t = default) =>
            await _telemetryBinder.SendTelemetryAsync(telemetryMessage, t);

        public async Task<long> UpdateReportedPropertiesAsync(ReportedProperties reportedProperties, CancellationToken ct = default) =>
            await _updateTwinBinder.InvokeAsync(_mqttClient.Options.ClientId, reportedProperties._properties, ct);
    }
}
