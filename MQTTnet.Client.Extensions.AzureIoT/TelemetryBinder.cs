using MQTTnet.Extensions.ManagedClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace MQTTnet.Client.Extensions.AzureIoT
{
    public  class TelemetryBinder
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

        public TelemetryBinder(IManagedMqttClient managedMqttClient) : this (managedMqttClient.InternalClient)
        {
            _telemetryQueueEnabled = true;
            _managedMqttClient = managedMqttClient;
        }

        public async Task SendTelemetryAsync(TelemetryMessage telemetryMessage, CancellationToken t = default)
        {
            MqttApplicationMessage msg = new MqttApplicationMessageBuilder()
                .WithTopic($"devices/{_mqttClient.Options.ClientId}/messages/events/$.ct=application%2Fjson&$.ce=utf-8")
                .WithPayload(_serializer.ToBytes(telemetryMessage.Payload))
                .WithRetainFlag(false)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            if (_telemetryQueueEnabled)
            {
                await _managedMqttClient.EnqueueAsync(msg);
            }
            else
            {
                if (_mqttClient.IsConnected)
                {
                    await _mqttClient.PublishAsync(msg, t);
                }
                else
                {
                    Trace.TraceWarning("Telemetry lost");
                }
            }
        }
    }
}
