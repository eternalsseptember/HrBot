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
        public static IApplicationBuilder UseTelegramBotWebHook(this IApplicationBuilder applicationBuilder, string webHookAddress)
        {
            var services = applicationBuilder.ApplicationServices;

            // Pretty sure that configuration could be made in IHostedService, but don't want to touch it without debugging
            var lifetime = services.GetRequiredService<IHostApplicationLifetime>();

            lifetime.ApplicationStarted.Register(
                async () =>
                {
                    var logger = services.GetRequiredService<ILogger<Startup>>();

                    logger.LogInformation("Removing WebHook");
                    await services.GetRequiredService<ITelegramBotClient>().DeleteWebhook();

                    logger.LogInformation($"Setting WebHook to {webHookAddress}");
                    await services.GetRequiredService<ITelegramBotClient>().SetWebhook(webHookAddress, maxConnections: 5);
                    logger.LogInformation($"WebHook is set to {webHookAddress}");

                    var webHookInfo = await services.GetRequiredService<ITelegramBotClient>().GetWebhookInfo();
                    logger.LogInformation($"WebHook info: {JsonConvert.SerializeObject(webHookInfo)}");
                });

            lifetime.ApplicationStopping.Register(
                () =>
                {
                    var logger = services.GetService<ILogger<Startup>>();

                    services.GetRequiredService<ITelegramBotClient>().DeleteWebhook().Wait();
                    logger.LogInformation("WebHook removed");
                });

            return applicationBuilder;
        }
    }
}