﻿using MQTTnet.Client.Extensions.AzureIoT.Binders.Serializer;
using MQTTnet.Extensions.ManagedClient;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Client.Extensions.AzureIoT.Binders
{
    public class TelemetryBinder
    {
        IMqttClient _mqttClient;
        IManagedMqttClient _managedMqttClient;
        IMessageSerializer _serializer;
        private readonly bool _telemetryQueueEnabled = false;

        public TelemetryBinder(IMqttClient mqttClient)
        {
            _mqttClient = mqttClient;
            _serializer = new Utf8JsonSerializer();
        }

        public TelemetryBinder(IManagedMqttClient managedMqttClient) : this(managedMqttClient.InternalClient)
        {
            _telemetryQueueEnabled = true;
            _managedMqttClient = managedMqttClient;
        }

        public async Task SendTelemetryAsync(TelemetryMessage telemetryMessage, CancellationToken t = default)
        {
            string topic = $"devices/{_mqttClient.Options.ClientId}/messages/events/" +
                           $"$.ct={System.Web.HttpUtility.UrlEncode(_serializer.ContentType)}&$.ce={_serializer.ContentEncoding}";
            MqttApplicationMessage msg = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(_serializer.ToBytes(telemetryMessage.Payload))
                .WithRetainFlag(false)
                .WithQualityOfServiceLevel(Protocol.MqttQualityOfServiceLevel.AtMostOnce)
                .Build();

            if (_telemetryQueueEnabled)
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
                //if (_mqttClient.IsConnected)
                //{
                //}
                //else
                //{
                //    Trace.TraceWarning("Telemetry lost");
                //}
            }
        }
    }
}