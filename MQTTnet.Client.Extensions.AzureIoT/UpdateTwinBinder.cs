using MQTTnet.Client;
using System.Web;

namespace MQTTnet.Client.Extensions.AzureIoT
{
    public class UpdateTwinBinder<T> : RequestResponseBinder<T, int>
    {
        public UpdateTwinBinder(IMqttClient c) : base(c, string.Empty, true)
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
                    var qs = HttpUtility.ParseQueryString(segments[segments.Length]);
                    if (int.TryParse(qs["$version"], out int v))
                    {
                        twinVersion = v;
                    }
                }
                return twinVersion;
            };
        }
    }
}