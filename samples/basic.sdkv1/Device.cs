using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using System.Text;
using System.Text.Json;

namespace basic.sdkv1
{
    public class Device : BackgroundService
    {
        private readonly ILogger<Device> _logger;
        private readonly IConfiguration _configuration;
        public Device(ILogger<Device> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var deviceClient = DeviceClient.CreateFromConnectionString(_configuration.GetConnectionString("cs"), TransportType.Mqtt);
            deviceClient.SetConnectionStatusChangesHandler((c, r) =>
            {
                _logger.LogWarning("Connection changed {c}, {r}", c, r);
            });
            await deviceClient.SetMethodDefaultHandlerAsync(async (mcb, ctx) =>
            {
                _logger.LogInformation("Cmd received: {c} with payload {p}", mcb.Name, mcb.DataAsJson);
                return await Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes("cmd ok"), 200));
            }, null);

            await deviceClient.SetDesiredPropertyUpdateCallbackAsync(async (dcb, ctx) =>
            {
                _logger.LogInformation("Prop received: {c} with payload {p}", dcb.Version, dcb.ToJson());
                await Task.Yield();
            }, null, stoppingToken);

            var twin = await deviceClient.GetTwinAsync(stoppingToken);
            _logger.LogInformation("twin reported version: {r}. desired version {d}", twin.Properties.Reported.Version, twin.Properties.Desired.Version);
            _logger.LogInformation("twin reported {r}, desired: {d}", twin.Properties.Reported.ToJson(), twin.Properties.Desired.ToJson());

            var reported = new TwinCollection();
            reported["started"] = DateTime.Now;
            await deviceClient.UpdateReportedPropertiesAsync(reported);

            var twin2 = await deviceClient.GetTwinAsync(stoppingToken);
            _logger.LogInformation("twin version: {r}", twin2.Version);
            _logger.LogInformation("twin json {d}", twin2.ToJson());

            int counter = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Sending Telemetry: {c}", ++counter);

                var msg = new Message(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { counter, Environment.WorkingSet })))
                {
                    ContentEncoding = "utf-8",
                    ContentType = "application/json"
                };


                await deviceClient.SendEventAsync(msg, stoppingToken);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}