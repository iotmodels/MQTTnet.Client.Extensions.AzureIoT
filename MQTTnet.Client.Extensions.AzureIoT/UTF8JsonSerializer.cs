using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MQTTnet.Client.Extensions.AzureIoT
{
    public class Utf8JsonSerializer : IMessageSerializer
    {
        private static class Json
        {
            public static string Stringify(object o) => JsonSerializer.Serialize(o);
            public static T FromString<T>(string s) => JsonSerializer.Deserialize<T>(s);
        }

        public byte[] ToBytes<T>(T payload)
        {
            if (payload == null) return new byte[0];
            return Encoding.UTF8.GetBytes(Json.Stringify(payload));
        }

        public bool TryReadFromBytes<T>(byte[] payload, out T result)
        {
            if (payload == null || payload.Length == 0)
            {
                result = default;
                return true;
            }
            result = Json.FromString<T>(Encoding.UTF8.GetString(payload));
            return true;
        }
    }
}