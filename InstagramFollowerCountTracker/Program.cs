namespace InstagramFollowerCountTracker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            // Bind the command-line arguments to WorkerOptions and add to DI
            builder.Services.Configure<WorkerOptions>(options => options.Args = args);

            builder.Services.AddHostedService<Worker>();

            IHost host = builder.Build();
            host.Run();
        }
    }
}