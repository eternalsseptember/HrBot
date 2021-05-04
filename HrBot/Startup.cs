using HrBot.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HrBot.Services;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace HrBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        public IConfiguration Configuration { get; }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();
            services.AddOptions();

            services.Configure<BotOptions>(Configuration.GetSection("BotOptions"));
            services.Configure<ChatOptions>(Configuration.GetSection("ChatOptions"));

            services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(Configuration["ChatOptions:BotToken"]));

            services.AddTransient<IMessageAnalyzer, MessageAnalyzer>();
            services.AddTransient<IVacancyReposter, VacancyReposter>();
            services.AddTransient<IVacancyAnalyzer, VacancyAnalyzer>();
            services.AddSingleton<IRepostedMessagesStorage, RepostedMessagesStorage>();
            services.AddTransient<IRepostedMessagesMonitoringService, RepostedMessagesMonitoringService>();

            services.AddMemoryCache();
        }


        public void Configure(IApplicationBuilder app, IOptions<BotOptions> appSettingsOptions)
        {
            app.UseTelegramBotWebHook(appSettingsOptions.Value.WebHookAddress);
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
