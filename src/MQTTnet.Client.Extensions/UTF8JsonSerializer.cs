using System.Text.Json;

namespace MQTTnet.Client.Extensions
{
    public class Utf8JsonSerializer : IMessageSerializer
    {
        public string ContentType => "application/json";
        public string ContentEncoding => "utf-8";

        public T FromBytes<T>(byte[] payload)
        {
            var reader = new Utf8JsonReader(payload);
            return JsonSerializer.Deserialize<T>(ref reader);
        }
        public byte[] ToBytes<T>(T payload) => JsonSerializer.SerializeToUtf8Bytes(payload);
    }
}