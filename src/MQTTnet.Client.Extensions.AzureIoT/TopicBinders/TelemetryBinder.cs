using MQTTnet.Extensions.ManagedClient;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Client.Extensions.AzureIoT.TopicBinders
{
    public class TelemetryBinder
    {
        private readonly IMqttClient _mqttClient;
        private readonly IManagedMqttClient _managedMqttClient;
        private readonly IMessageSerializer _serializer;

        public TelemetryBinder(IMqttClient mqttClient)
        {
            _mqttClient = mqttClient;
            _serializer = new Utf8JsonSerializer();
        }

        public TelemetryBinder(IManagedMqttClient managedMqttClient) : this(managedMqttClient.InternalClient)
        {
            _managedMqttClient = managedMqttClient;
        }

        public async Task SendTelemetryAsync(TelemetryMessage telemetryMessage, CancellationToken t = default)
        {
            string topic = $"devices/{_mqttClient.Options.ClientId}/messages/events/" +
                           $"$.ct={System.Web.HttpUtility.UrlEncode(_serializer.ContentType)}&" +
                           $"$.ce={_serializer.ContentEncoding}";

            MqttApplicationMessage msg = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithContentType(_serializer.ContentType)
                .WithPayload(_serializer.ToBytes(telemetryMessage.Payload))
                .WithRetainFlag(false)
                .WithQualityOfServiceLevel(Protocol.MqttQualityOfServiceLevel.AtMostOnce)
                .Build();

            if (_managedMqttClient != null)
            {
                ManagedMqttApplicationMessage mmsg = new ManagedMqttApplicationMessageBuilder()
                    .WithApplicationMessage(msg)
                    .WithId(new System.Guid())
                    .Build();
                await _managedMqttClient.EnqueueAsync(mmsg);
            }
            else
            {
                await _mqttClient.PublishAsync(msg, t);
            }
        }
    }
}
