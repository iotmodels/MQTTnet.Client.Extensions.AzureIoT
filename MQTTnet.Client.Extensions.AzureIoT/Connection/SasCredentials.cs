using System;
using System.Security.Cryptography;
using System.Text;

namespace MQTTnet.Client.Extensions.AzureIoT.Connection
{
    public class SasCredentials : IMqttClientCredentialsProvider
    {
        private const string apiversion_2020_09_30 = "2020-09-30";
        private readonly string _hostname;
        private readonly string _did;
        private readonly string _sasKey;
        private readonly string _modelId;
        private readonly int _sasMinutes;
        public SasCredentials(string hostname, string deviceid, string sasKey, int sasMinutes = 60, string modelId = "")
        {
            _hostname = hostname;
            _did = deviceid;
            _sasKey = sasKey;
            _modelId = modelId;
            _sasMinutes = sasMinutes;
        }
        public byte[] GetPassword(MqttClientOptions clientOptions) =>
            Encoding.UTF8.GetBytes(CreateSasToken($"{_hostname}/devices/{_did}", _sasKey));

        public string GetUserName(MqttClientOptions clientOptions) =>
            $"{_hostname}/{_did}/?api-version={apiversion_2020_09_30}&model-id={_modelId}";

        internal string Sign(string requestString, string key)
        {
            using (HMACSHA256 algorithm = new HMACSHA256(Convert.FromBase64String(key)))
            {
                return Convert.ToBase64String(algorithm.ComputeHash(Encoding.UTF8.GetBytes(requestString)));
            }
        }

        internal string CreateSasToken(string resource, string sasKey)
        {
            var expiry = DateTimeOffset.UtcNow.AddMinutes(_sasMinutes).ToUnixTimeSeconds().ToString();
            var sig = System.Net.WebUtility.UrlEncode(Sign($"{resource}\n{expiry}", sasKey));
            return $"SharedAccessSignature sr={resource}&sig={sig}&se={expiry}";
        }
    }
}
