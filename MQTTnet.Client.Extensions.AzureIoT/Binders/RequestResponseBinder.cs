using System;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet.Client.Extensions.AzureIoT.Binders.Serializer;

namespace MQTTnet.Client.Extensions.AzureIoT.Binders
{
    public class RequestResponseBinder<T, TResp>
    {
        readonly IMqttClient mqttClient;
        TaskCompletionSource<TResp> tcs;

        protected string requestTopicPattern = string.Empty; 
        protected string responseTopicSub = string.Empty; 
        protected string responseTopicSuccess = string.Empty; 
        protected string responseTopicFailure = string.Empty; 
        protected bool requireNotEmptyPayload = true;
        string remoteClientId = string.Empty;
        Guid corr = Guid.NewGuid();

        protected Func<string, TResp> VersionExtractor { get; set; }

        readonly IMessageSerializer _serializer;

        //public RequestResponseBinder(IMqttClient client)
        //    : this(client, new Utf8JsonSerializer())
        //{

        //}

        public RequestResponseBinder(IMqttClient client, IMessageSerializer serializer)
        {
            mqttClient = client;
            _serializer = serializer;
            mqttClient.ApplicationMessageReceivedAsync += async m =>
            {
                var topic = m.ApplicationMessage.Topic;
                var expectedTopic = responseTopicSuccess.Replace("{clientId}", remoteClientId);
                if (topic.StartsWith(expectedTopic))
                {
                    if (m.ApplicationMessage.CorrelationData != null && corr != new Guid(m.ApplicationMessage.CorrelationData))
                    {
                        tcs.SetException(new ApplicationException("Invalid correlation data"));
                    }

                    if (requireNotEmptyPayload)
                    {
                        TResp resp = _serializer.FromBytes<TResp>(m.ApplicationMessage.Payload);
                        tcs.SetResult(resp);
                    }
                    else
                    {
                        // update twin returns version from topic response
                        TResp resp = VersionExtractor.Invoke(topic);
                        tcs.SetResult(resp);
                    }
                }

                await Task.Yield();
            };
        }
        public async Task<TResp> InvokeAsync(string clientId, T request, CancellationToken ct = default)
        {
            tcs = new TaskCompletionSource<TResp>();
            remoteClientId = clientId;
            string commandTopic = requestTopicPattern.Replace("{clientId}", remoteClientId);
            var responseTopic = responseTopicSub.Replace("{clientId}", remoteClientId);
            await mqttClient.SubscribeAsync(responseTopic, Protocol.MqttQualityOfServiceLevel.AtMostOnce, ct);

            MqttApplicationMessage msg = new MqttApplicationMessage()
            {
                Topic = commandTopic,
                Payload = _serializer.ToBytes(request),
                ResponseTopic = responseTopicSuccess.Replace("{clientId}", remoteClientId),
                CorrelationData = corr.ToByteArray()
            };
            var pubAck = await mqttClient.PublishAsync(msg);
            if (!pubAck.IsSuccess)
            {
                throw new ApplicationException("Error publishing Request Message");
            }
            return await tcs.Task.TimeoutAfter(TimeSpan.FromSeconds(5));
        }
    }
}