using System.Threading.Tasks;
using HrBot.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace HrBot
{
    public class Program
    {
        private const string FileTemplate
            = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] <s:{SourceContext}> {Message}{NewLine}{Exception}";

        private const string ConsoleTemplate
            = "[{Timestamp:HH:mm:ss} {Level:u3}] <s:{SourceContext}> {Message:lj}{NewLine}{Exception}";

        public static async Task Main(string[] args)
            => await CreateHostBuilder(args).Build().RunAsync();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(
                    (hostingContext, config) =>
                    {
                        var environment = hostingContext.HostingEnvironment.EnvironmentName;

                        config.AddJsonFile("Configuration/appsettings.json", false, true);
                        config.AddJsonFile($"Configuration/appsettings.{environment}.json", true, true);
                    })
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
                .ConfigureServices(x => x.AddHostedService<RepostedMessagesMonitoringHostedService>())
                .ConfigureLogging(
                    (context, builder) =>
                    {
                        builder.ClearProviders();
                        builder.AddSerilog(
                            dispose: true,
                            logger: new LoggerConfiguration()
                                .Enrich.FromLogContext()
                                .MinimumLevel.Verbose()
                                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                                .MinimumLevel.Override("System", LogEventLevel.Warning)
                                .WriteTo.Console(
                                    outputTemplate: ConsoleTemplate,
                                    restrictedToMinimumLevel: LogEventLevel.Verbose)
                                .WriteTo.File(
                                    "logs/all-.log",
                                    rollingInterval: RollingInterval.Day,
                                    outputTemplate: FileTemplate,
                                    restrictedToMinimumLevel: LogEventLevel.Verbose)
                                .WriteTo.File(
                                    "logs/information-.log",
                                    rollingInterval: RollingInterval.Day,
                                    outputTemplate: FileTemplate,
                                    restrictedToMinimumLevel: LogEventLevel.Information)
                                .CreateLogger());
                    });
    }
}
