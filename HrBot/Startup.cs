using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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

            services.Configure<AppSettings>(Configuration.GetSection("Configuration"));

            services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(Configuration["Configuration:BotToken"]));

            services.AddTransient<IVacancyReposter, VacancyReposter>();
            services.AddTransient<IVacancyAnalyzer, VacancyAnalyzer>();
            services.AddMemoryCache();
            services.AddSingleton<IRepostedMessagesStorage, RepostedMessagesStorage>();
            services.AddTransient<IRepostedMessagesMonitoringService, RepostedMessagesMonitoringService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<AppSettings> appSettings)
        {
            app.UseTelegramBotWebHook();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
