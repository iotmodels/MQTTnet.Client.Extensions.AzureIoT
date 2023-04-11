using MQTTnet.Client.Extensions.AzureIoT;
using System.Text.Json;

namespace SampleIoTHubClient
{
    internal class Program
    {
        static async Task Main()
        {
            string hostname = "tests.azure-devices.net";
            string did = "testdevice";
            string sas = "MDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDA=";
            var deviceClient = await HubDeviceClient.CreateManagedClientAsync(hostname, did, sas, TimeSpan.FromSeconds(2));
            var twin = await deviceClient.GetTwinAsync();
            await Console.Out.WriteLineAsync(twin);


            deviceClient.OnCommandReceived += async cmd =>
            {
                await Console.Out.WriteLineAsync("received cmd:" + cmd.CommandName);
                string? payload = JsonSerializer.Deserialize<string>(cmd.CommandPayload);
                return new CommandResponse() { Status = 200, ReponsePayload = payload + payload };
            };

            while (true)
            {
                await deviceClient.EnqueTelemetryAsync(did, new { Environment.WorkingSet });
                await Task.Delay(100000);
            }
        }
    }
}