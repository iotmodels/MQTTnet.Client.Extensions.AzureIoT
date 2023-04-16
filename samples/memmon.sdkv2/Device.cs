using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using MQTTnet;

namespace memmon.device
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
            string connectionString = _configuration.GetConnectionString("cs")!;

            var deviceClient = await ClientFactory.CreateFromConnectionStringAsync(connectionString, _logger);

            await deviceClient.SetDirectMethodCallbackAsync(async m =>
            {
                _logger.LogInformation("Cmd received: {c} with payload {p}", m.MethodName, m.GetPayloadAsJsonString());
                return await Task.FromResult(new DirectMethodResponse(200) { Payload = "ok response" });
            }, stoppingToken);

            await deviceClient.SetDesiredPropertyUpdateCallbackAsync(async m =>
            {
                _logger.LogInformation("prop received {p}", m.GetSerializedString());
                await Task.Yield();
            }, stoppingToken);

            var twin = await deviceClient.GetTwinPropertiesAsync(stoppingToken);
            _logger.LogInformation("twin reported: {r}, desired: {d}", twin.Reported.Version, twin.Desired.Version);
            _logger.LogInformation("twin reported: {r}, desired: {d}", twin.Reported.GetSerializedString(), twin.Desired.GetSerializedString());

            var reportedProperties = new ReportedProperties();
            reportedProperties["started"] = DateTime.UtcNow;
            var v = await deviceClient.UpdateReportedPropertiesAsync(reportedProperties, stoppingToken);
            _logger.LogInformation("updated started to: {v}", v);

            twin = await deviceClient.GetTwinPropertiesAsync(stoppingToken);
            _logger.LogInformation("twin reported: {r}, desired: {d}", twin.Reported.Version, twin.Desired.Version);
            _logger.LogInformation("twin reported: {r}, desired: {d}", twin.Reported.GetSerializedString(), twin.Desired.GetSerializedString());

            int counter = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Sending Telemetry: {c}", counter++);
                await deviceClient.SendTelemetryAsync(new TelemetryMessage(new { counter, Environment.WorkingSet }), stoppingToken);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}