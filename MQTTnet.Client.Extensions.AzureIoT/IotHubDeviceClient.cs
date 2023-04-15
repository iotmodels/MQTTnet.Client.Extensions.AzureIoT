using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Client.Extensions.AzureIoT
{
    public class IotHubDeviceClient
    {
        private readonly string _connectionString;
        private IMqttClient _mqttClient;

        private GetTwinBinder _getTwinBinder;
        private Command _commandBinder;
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
        }

        public async Task SetDirectMethodCallbackAsync(Func<DirectMethodRequest, Task<DirectMethodResponse>> userCallback)
        {
            _commandBinder.OnCmdDelegate = async req =>
            {
                var dmResp = await userCallback.Invoke(new DirectMethodRequest() { MethodName = req.CommandName, JsonPayload = req.CommandPayload });
                CommandResponse resp = new CommandResponse()
                {
                    ReponsePayload = dmResp.Payload
                };
                return await Task.FromResult(resp);
            };
            await Task.Yield();
        }

        public Task SetDesiredPropertyUpdateCallbackAsync(Func<DesiredProperties, Task> value)
        {
            throw new NotImplementedException();
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
    }
}
