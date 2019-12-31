using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

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
                        .MinimumLevel.Verbose()
                        .Enrich.FromLogContext()
                        .ReadFrom.Configuration(context.Configuration);
                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        configuration.WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Verbose,
                            theme: AnsiConsoleTheme.Literate);
                    }
                });
    }
}