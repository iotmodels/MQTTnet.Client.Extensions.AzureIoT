using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Extensions.AzureIoT;
using MQTTnet.Client.Extensions.AzureIoT.Binders;
using MQTTnet.Client.Extensions.AzureIoT.Connection;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        IotHubDeviceClient deviceClient;
        public Form1()
        {
            InitializeComponent();
        }

        private void UpdateCommandText(string text)
        {
            textBoxCommands.Text = text;
        }

        private async void buttonConnect_Click(object sender, EventArgs e)
        {
            var logs = new List<string>();
            deviceClient = await CreateFromConnectionStringAsync(textBoxConnectionString.Text, logs);
            labelConnectionStatus.Text = string.Join(" ", logs);
            var twin = await deviceClient.GetTwinPropertiesAsync();
            textBoxTwin.Text = twin.Reported.GetSerializedString();
            textBoxDesired.Text = twin.Desired.GetSerializedString();

            await deviceClient.SetDirectMethodCallbackAsync(async req =>
            {
                if (textBoxCommands.InvokeRequired)
                {
                    //textBoxCommands.Invoke(UpdateCommandText, $"{req.MethodName} {req.JsonPayload}");
                }
                else
                {
                    UpdateCommandText($"{req.MethodName} {req.JsonPayload}");
                }
                return new DirectMethodResponse(200) { Payload = "cmd ok" };
            });

            await deviceClient.SetDesiredPropertyUpdateCallbackAsync(async prop =>
            {
                textBoxDesired.Text = prop.GetSerializedString();
                await Task.Yield();
            });
            timer1.Enabled = true;
        }

        public static Task<IotHubDeviceClient> CreateFromConnectionStringAsync(string connectionString,IList<string> logs)
        {
            var mqttClient = new MqttFactory().CreateManagedMqttClient(MqttNetTraceLogger.CreateTraceLogger());
            var tcs = new TaskCompletionSource<IotHubDeviceClient>();
            mqttClient.ConnectedAsync += async c =>
            {
                var client = new IotHubDeviceClient(mqttClient)
                {
                    ConnectionStatusChangeCallback = conn => logs.Add($"Connection status changed: {conn.Status}")
                };
                tcs.TrySetResult(client);
                await Task.Yield();
            };
            var cs = new ConnectionSettings(connectionString);
            mqttClient.StartAsync(new ManagedMqttClientOptionsBuilder()
                .WithClientOptions(new MqttClientOptionsBuilder().WithConnectionSettings(cs).Build())
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .Build());
            return tcs.Task.TimeoutAfter(TimeSpan.FromSeconds(30));
        }

        private async void timer1_Tick(object sender, EventArgs e)
        {
            textBoxTelemetry.Text += "Sending Telemetry.. \n";
            await deviceClient.SendTelemetryAsync(new TelemetryMessage( new { Environment.WorkingSet }));
        }
    }
}

