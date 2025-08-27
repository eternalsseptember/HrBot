using HrBot.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

            services.Configure<AppSettings>(Configuration.GetSection("Configuration"));

            services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(Configuration["Configuration:BotToken"]!));

            services.AddTransient<IVacancyReposter, VacancyReposter>();
            services.AddTransient<IVacancyAnalyzer, VacancyAnalyzer>();
            services.AddMemoryCache();
            services.AddSingleton<IRepostedMessagesStorage, RepostedMessagesStorage>();
            services.AddTransient<IRepostedMessagesMonitoringService, RepostedMessagesMonitoringService>();
        }

        public void Configure(IApplicationBuilder app, IOptions<AppSettings> appSettingsOptions)
        {
            app.UseTelegramBotWebHook(appSettingsOptions.Value.WebHookAddress);
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
