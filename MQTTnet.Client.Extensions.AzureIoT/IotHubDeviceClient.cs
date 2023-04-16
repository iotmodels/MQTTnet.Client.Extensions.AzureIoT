using MQTTnet.Extensions.ManagedClient;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Client.Extensions.AzureIoT
{
    public class IotHubDeviceClient
    {
        private readonly string _connectionString;
        private readonly IMqttClient _mqttClient;
        private readonly IManagedMqttClient _managedMqttClient = null;
        private bool _telemetryQueueEnabled = false;

        private GetTwinBinder _getTwinBinder;
        private UpdateTwinBinder<object> _updateTwinBinder;
        private DesiredUpdatePropertyBinder _desiredUpdateBinder;
        private Command _commandBinder;

        public Action<ConnectionStatusInfo> ConnectionStatusChangeCallback { get; set; }

        public IotHubDeviceClient(IMqttClient mqttClient)
        {
            _mqttClient = mqttClient;
            _getTwinBinder = new GetTwinBinder(_mqttClient);
            _commandBinder = new Command(_mqttClient);
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
                        d.ConnectResult.ResultCode.ToConnectionStatusChangeReason() ,
                    Status = d.ConnectResult == null ?
                        ConnectionStatus.Disconnected :
                        d.ConnectResult.ResultCode.ToConnectionStatus()
                }) ;
                await Task.Yield();
            };
        }

        public IotHubDeviceClient(IManagedMqttClient mqttClient) : this(mqttClient.InternalClient)
        {
            _telemetryQueueEnabled = true;
            _managedMqttClient = mqttClient;
        }

        public IotHubDeviceClient(string connectionString) : this(new MqttFactory().CreateMqttClient(MqttNetTraceLogger.CreateTraceLogger()))
        {
            _connectionString = connectionString;
        }

        public async Task OpenAsync(CancellationToken ct = default)
        {
            var cs = new ConnectionSettings(_connectionString);
            await _mqttClient.ConnectAsync(new MqttClientOptionsBuilder().WithConnectionSettings(cs).Build(), ct);

            _getTwinBinder = new GetTwinBinder(_mqttClient);
            _commandBinder = new Command(_mqttClient);
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

        public async Task SendTelemetryAsync(TelemetryMessage telemetryMessage, CancellationToken t = default)
        {
            if (_telemetryQueueEnabled)
            {
                await _managedMqttClient.EnqueueAsync(
                    new ManagedMqttApplicationMessageBuilder()
                    .WithApplicationMessage(new MqttApplicationMessageBuilder()
                        .WithTopic($"devices/{_mqttClient.Options.ClientId}/messages/events/")
                        .WithPayload(new Utf8JsonSerializer().ToBytes(telemetryMessage.Payload))
                        .Build())
                    .Build());
            }
            else
            {
                if (_mqttClient.IsConnected)
                {
                    await _mqttClient.PublishBinaryAsync($"devices/{_mqttClient.Options.ClientId}/messages/events/",
                      new Utf8JsonSerializer().ToBytes(telemetryMessage.Payload),
                      Protocol.MqttQualityOfServiceLevel.AtLeastOnce,
                      false, t);
                }
                else
                {
                    Trace.TraceWarning("Telemetry lost");
                }
            }
        }

        public async Task<long> UpdateReportedPropertiesAsync(ReportedProperties reportedProperties, CancellationToken ct = default)
        {
            var twin = await _updateTwinBinder.InvokeAsync(_mqttClient.Options.ClientId, reportedProperties, ct);
            return twin;
        }
    }
}
