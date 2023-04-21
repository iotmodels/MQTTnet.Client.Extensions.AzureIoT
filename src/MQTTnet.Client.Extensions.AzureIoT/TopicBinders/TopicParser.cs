using System;
using System.Web;

namespace MQTTnet.Client.Extensions.AzureIoT.TopicBinders
{
    public class TopicParser
    {
        public static TopicParameters ParseTopic(string topic)
        {
            var segments = topic.Split('/');
            int twinVersion = -1;
            string rid = string.Empty;
            if (topic.Contains("?"))
            {
                var qs = HttpUtility.ParseQueryString(segments[segments.Length - 1]);
                if (int.TryParse(qs["$version"], out int v))
                {
                    twinVersion = v;
                }
                rid = Convert.ToString(qs["$rid"]);
            }
            return new TopicParameters() { Rid = rid, Version = twinVersion };
        }
    }
}
