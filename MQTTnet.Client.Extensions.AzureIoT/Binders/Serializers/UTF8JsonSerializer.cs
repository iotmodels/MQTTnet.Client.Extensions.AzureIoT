using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MQTTnet.Client.Extensions.AzureIoT.Binders.Serializer
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