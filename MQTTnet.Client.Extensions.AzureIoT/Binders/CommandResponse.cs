using System.Text.Json.Serialization;

namespace MQTTnet.Client.Extensions.AzureIoT.Binders
{
    public class CommandResponse
    {
        [JsonIgnore]
        public int Status { get; set; }
        [JsonPropertyName("payload")]
        public string ReponsePayload { get; set; }
    }
}
