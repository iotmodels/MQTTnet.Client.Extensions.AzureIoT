using memmon.device;

namespace memmon.sdklite
{
    public class Program
    {
        public static void Main(string[] args)
        {
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