using MQTTnet.Adapter;
using MQTTnet.Client.Extensions.AzureIoT;
using MQTTnet.Client.Extensions.AzureIoT.Auth;
using System.Text;

namespace MQTTnet.Client.Extensions.IntegrationTests
{
    public class IoTHubDeviceClientTests
    {
        [Fact]
        public void NullConnectionThrowsException()
        {
            MqttClient? mqttClient = null;
            Assert.Throws<ArgumentNullException>(() => new IotHubDeviceClient(mqttClient));
        }

        [Fact]
        public void ClosedConnectionThrowsException()
        {
            var mqttClient = new MqttFactory().CreateMqttClient(MqttNetTraceLogger.CreateTraceLogger());
            Assert.Throws<ArgumentException>(() => new IotHubDeviceClient(mqttClient));
        }

        [Fact]
        public async Task ConnectTestClient()
        {
            var mqttClient = new MqttFactory().CreateMqttClient(MqttNetTraceLogger.CreateTraceLogger());
            var cs = new IoTHubConnectionSettings
            {
                HostName = "tests.azure-devices.net",
                DeviceId = "testdevice",
                SharedAccessKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.Empty.ToString("N")))
            };
            await mqttClient.ConnectAsync(new MqttClientOptionsBuilder().WithIoTHubConnectionSettings(cs).Build());
            Assert.True(mqttClient.IsConnected);
            var deviceClient = new IotHubDeviceClient(mqttClient){};
            await mqttClient.DisconnectAsync();
    }

    [Fact]
        public async Task BadCredentialThrowsException()
        {
            var mqttClient = new MqttFactory().CreateMqttClient(MqttNetTraceLogger.CreateTraceLogger());
            var cs = new IoTHubConnectionSettings
            {
                HostName = "tests.azure-devices.net",
                DeviceId = "bad-cred",
                SharedAccessKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.Empty.ToString("N")))
            };
            await Assert.ThrowsAsync<MqttConnectingFailedException>(async () => 
                await mqttClient.ConnectAsync(new MqttClientOptionsBuilder().WithIoTHubConnectionSettings(cs).Build()));
        }

    }
}