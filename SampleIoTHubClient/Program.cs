using MQTTnet.Client.Extensions.AzureIoT;
using System.Text.Json;

namespace SampleIoTHubClient
{
    internal class Program
    {
        static async Task Main()
        {
            string hostname = "tests.azure-devices.net";
            string did = "mqttnetclient";
            string sas = "MDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDA=";
            var deviceClient = await HubDeviceClient.CreateManagedClientAsync(hostname, did, sas, TimeSpan.FromSeconds(2));

            deviceClient._managedMqttClient.InternalClient.DisconnectedAsync += async e => await Console.Out.WriteLineAsync(deviceClient._managedMqttClient.InternalClient.IsConnected.ToString());
            deviceClient._managedMqttClient.InternalClient.ConnectedAsync += async e => await Console.Out.WriteLineAsync(deviceClient._managedMqttClient.InternalClient.IsConnected.ToString());

            var twin = await deviceClient.GetTwinAsync();
            await Console.Out.WriteLineAsync(twin);

            deviceClient.OnCommandReceived += async cmd =>
            {
                await Console.Out.WriteLineAsync("received cmd:" + cmd.CommandName);
                string? payload = JsonSerializer.Deserialize<string>(cmd.CommandPayload);
                return new CommandResponse() { Status = 200, ReponsePayload = payload + payload };
            };

            deviceClient.OnPropertyUpdateReceived += prop =>
            {
                Console.WriteLine("received prop:" + prop.ToString());
                string? payload = prop.ToJsonString();
                return new PropertyAck() { Status = 200, Version = prop["$version"]!.GetValue<int>(), Value = prop };
            };

            await deviceClient.UpdateTwinAsync(new { DateTime.Now });
            var twin2 = await deviceClient.GetTwinAsync();
            await Console.Out.WriteLineAsync(twin2);

            while (true)
            {
                await deviceClient.EnqueTelemetryAsync(did, new { Environment.WorkingSet });
                await Task.Delay(100000);
            }
        }
    }
}