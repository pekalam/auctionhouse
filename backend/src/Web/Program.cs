using System.Diagnostics;
using System.Threading;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog((context, configuration) =>
                {
                    configuration
                        .Destructure.ToMaximumStringLength(1000)
                        .Destructure.ToMaximumCollectionCount(40)
                        .MinimumLevel.Verbose()
                        .Enrich.FromLogContext()
                        .ReadFrom.Configuration(context.Configuration);
                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        configuration.WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Verbose,
                            theme: AnsiConsoleTheme.Literate);
                    }
                });

        public static void Shutdown()
        {
            Process.GetCurrentProcess().Kill();
        }
    }
}