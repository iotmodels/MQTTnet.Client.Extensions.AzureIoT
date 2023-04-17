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
