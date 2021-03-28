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
            services.AddTransient<AppSettings>(ser => ser.GetRequiredService<IOptions<AppSettings>>().Value);

            services.AddSingleton<ITelegramBotClient>(
                x =>
                {
                    var settings = x.GetRequiredService<AppSettings>();
                    return new TelegramBotClient(settings.BotToken);
                });

            services.AddTransient<IVacancyReposter, VacancyReposter>();
            services.AddTransient<IVacancyAnalyzer, VacancyAnalyzer>();
            services.AddMemoryCache();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseTelegramBotWebHook();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
