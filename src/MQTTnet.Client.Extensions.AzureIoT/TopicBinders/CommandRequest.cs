namespace MQTTnet.Client.Extensions.AzureIoT.TopicBinders
{
    public class CommandRequest
    {
        public string CommandName { get; set; }
        public string CommandPayload { get; set; }
    }
}
