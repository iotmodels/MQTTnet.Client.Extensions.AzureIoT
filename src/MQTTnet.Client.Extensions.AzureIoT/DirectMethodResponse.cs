namespace MQTTnet.Client.Extensions.AzureIoT
{
    public class DirectMethodResponse
    {
        public int Status { get; set; }

        public DirectMethodResponse(int status)
        {
            Status = status;
        }

        public string Payload { get; set; }
    }
}