using MQTTnet.Client.Extensions.AzureIoT.Binders.Serializer;

namespace MQTTnet.Client.Extensions.AzureIoT.Binders
{
    public class UpdateTwinBinder<T> : RequestResponseBinder<object, int>
    {
        public UpdateTwinBinder(IMqttClient c) : base(c, new Utf8JsonSerializer())
        {
            var rid = RidCounter.NextValue();
            requestTopicPattern = $"$iothub/twin/PATCH/properties/reported/?$rid={rid}";
            responseTopicSub = "$iothub/twin/res/#";
            responseTopicSuccess = $"$iothub/twin/res/204/?$rid={rid}";
            requireNotEmptyPayload = false;
            VersionExtractor = topic =>
            {
                var segments = topic.Split('/');
                int twinVersion = -1;
                if (topic.Contains("?"))
                {
                    var tp = TopicParser.ParseTopic(topic);
                    twinVersion = tp.Version;

                }
                return twinVersion;
            };
        }
    }
}