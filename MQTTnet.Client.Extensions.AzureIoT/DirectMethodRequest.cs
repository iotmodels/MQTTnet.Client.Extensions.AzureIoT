namespace MQTTnet.Client.Extensions.AzureIoT
{
    public class DirectMethodRequest
    {
        public string MethodName { get; set; }

        public string JsonPayload { get; set; }

        public string GetPayloadAsJsonString()
        {
            return JsonPayload;
        }
    }
}
