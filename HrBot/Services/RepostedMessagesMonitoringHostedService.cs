using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HrBot.Services
{
    public class RepostedMessagesMonitoringHostedService : IHostedService
    {
        private readonly ILogger<RepostedMessagesMonitoringHostedService> _logger;
        private readonly IRepostedMessagesMonitoringService _repostedMessagesMonitoringService;
        private readonly Timer _timer;


        public RepostedMessagesMonitoringHostedService(
            IRepostedMessagesMonitoringService repostedMessagesMonitoringService,
            ILogger<RepostedMessagesMonitoringHostedService> logger)
        {
            _logger = logger;
            _repostedMessagesMonitoringService = repostedMessagesMonitoringService;
            _timer = new Timer(OnTimer, default, Timeout.Infinite, Timeout.Infinite);
        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer.Change(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

            return Task.CompletedTask;
        }


        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _timer.DisposeAsync();
        }


        private async void OnTimer(object? _)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            try
            {
                await _repostedMessagesMonitoringService.RemoveDeletedMessagesFromChannel();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An exception is occurred during message monitoring: {Message}", e.Message);
            }

            _timer.Change(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }
    }
}