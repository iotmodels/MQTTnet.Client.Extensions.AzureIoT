//using Microsoft.Azure.Devices.Client;

namespace MQTTnet.Client.Extensions.AzureIoT
{
    public class DirectMethodResponse
    {
        private int v;

        public DirectMethodResponse(int v)
        {
            this.v = v;
        }

        public string Payload { get; set; }
    }
}