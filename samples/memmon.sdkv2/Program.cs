using memmon.device;
using System.Diagnostics;

namespace memmon.sdkv2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Trace.Listeners.Add(new ConsoleTraceListener());
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddHostedService<Device>();
                })
                .Build();

            host.Run();
        }
    }
}