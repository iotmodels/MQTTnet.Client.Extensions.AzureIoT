using System.Text.Json;

namespace MQTTnet.Client.Extensions.AzureIoT.TopicBinders
{
    internal class GetTwinBinder : RequestResponseBinder<string, JsonElement>
    {
        internal int lastRid = 0;
        public GetTwinBinder(IMqttClient client) : base(client)
        {
            var rid = RidCounter.NextValue();
            lastRid = rid;
            requestTopicPattern = $"$iothub/twin/GET/?$rid={rid}";
            responseTopicSub = "$iothub/twin/res/#";
            responseTopicSuccess = $"$iothub/twin/res/200/?$rid={rid}";
        }
    }
}