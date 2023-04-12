using MQTTnet.Adapter;
using MQTTnet.Client;
using MQTTnet.Client.Extensions.AzureIoT;
using MQTTnet.Extensions.ManagedClient;

namespace HubClientTests
{
    public class ConnectionFixture
    {
        [Fact]
        public async Task ConnectWithValidCredentials()
        {
            string hostname = "tests.azure-devices.net";
            string did = "testdevice";
            string sas = "MDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDA=";
        
            var client = await HubDeviceClient.CreateClientAsync(hostname, did, sas);
            var connected = client._mqttClient.IsConnected;
            Assert.True(connected);
            await client._mqttClient.DisconnectAsync(new MqttClientDisconnectOptionsBuilder().WithReasonString("test").Build());
        }

        [Fact]
        public async Task ConnectWithInValidCredentials()
        {
            string hostname = "tests.azure-devices.net";
            string did = "testdevice-invalid";
            string sas = "MDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDA=";

            await Assert.ThrowsAsync<MqttConnectingFailedException>(
                async () => await HubDeviceClient.CreateClientAsync(hostname, did, sas));
        }

    }
}