namespace memmon.sdkv2
{
    internal static class ClientFactory
    {

        public static async Task<IotHubDeviceClient> CreateFromConnectionStringAsync(string connectionString, ILogger logger)
        {
            var client = new IotHubDeviceClient(connectionString)
            {
                ConnectionStatusChangeCallback = c => logger.LogWarning("Connection status changed: {s}", c.Status)
            };
            await client.OpenAsync();
            return client;
        }
    }
}