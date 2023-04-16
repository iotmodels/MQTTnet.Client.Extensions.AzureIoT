using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace MQTTnet.Client.Extensions.AzureIoT
{
    public class TwinProperties
    {
        public TwinProperties(JsonElement twinJson)
        {
            Reported = FillReported(twinJson);
            Desired = FillDesired(twinJson);
        }

        public TwinProperties(DesiredProperties desired, ReportedProperties reported)
        {
            Reported = reported;
            Desired = desired;
        }
        public ReportedProperties Reported { get; set; }
        public DesiredProperties Desired { get; set; }


        private DesiredProperties FillDesired(JsonElement twinJson)
        {
            var desiredProperties = new DesiredProperties();
            var desiredElement = twinJson.GetProperty("desired");
            foreach (var item in desiredElement.EnumerateObject())
            {
                if (item.Name == "$version")
                {
                    desiredProperties.Version = item.Value.Deserialize<long>();
                }
                else
                {
                    desiredProperties[item.Name] = item.Value;
                }
            }
            return desiredProperties;
        }

        private ReportedProperties FillReported(JsonElement twinJson)
        {
            var reportedProperties = new ReportedProperties();
            var reportedElement = twinJson.GetProperty("reported");
            foreach (var item in reportedElement.EnumerateObject())
            {
                if (item.Name == "$version")
                {
                    reportedProperties.Version = item.Value.Deserialize<long>();
                }
                else
                {
                    reportedProperties[item.Name] = item.Value;
                }
            }
            return reportedProperties;
        }


    }
}
