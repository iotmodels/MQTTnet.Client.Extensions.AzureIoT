using System;
using System.Collections.Generic;
using System.Text;

namespace MQTTnet.Client.Extensions.AzureIoT
{
    public class TelemetryMessage
    {
        public object Payload { get; set; }
        public TelemetryMessage(object payload)
        {
            Payload = payload;
        }
    }
}
