using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot;

namespace HrBot
{
    public static class StartupExtensions
    {
        public static IApplicationBuilder UseTelegramBotWebHook(this IApplicationBuilder applicationBuilder)
        {
            var services = applicationBuilder.ApplicationServices;

            var lifetime = services.GetRequiredService<IHostApplicationLifetime>();

            lifetime.ApplicationStarted.Register(
                async () =>
                {
                    var logger = services.GetRequiredService<ILogger<Startup>>();
                    var address = services.GetRequiredService<AppSettings>().WebHookAddress;

                    logger.LogInformation("Removing WebHook");
                    await services.GetRequiredService<ITelegramBotClient>().DeleteWebhookAsync();

                    logger.LogInformation($"Setting WebHook to {address}");
                    await services.GetRequiredService<ITelegramBotClient>().SetWebhookAsync(address, maxConnections: 5);
                    logger.LogInformation($"WebHook is set to {address}");

                    var webHookInfo = await services.GetRequiredService<ITelegramBotClient>().GetWebhookInfoAsync();
                    logger.LogInformation($"WebHook info: {JsonConvert.SerializeObject(webHookInfo)}");
                });

            lifetime.ApplicationStopping.Register(
                () =>
                {
                    var logger = services.GetService<ILogger<Startup>>();

                    services.GetRequiredService<ITelegramBotClient>().DeleteWebhookAsync().Wait();
                    logger.LogInformation("WebHook removed");
                });

            return applicationBuilder;
        }
    }
}