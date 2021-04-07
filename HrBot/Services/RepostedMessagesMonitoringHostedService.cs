using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace HrBot.Services
{
    public class RepostedMessagesMonitoringHostedService : IHostedService, IDisposable
    {
        public RepostedMessagesMonitoringHostedService(IRepostedMessagesMonitoringService repostedMessagesMonitoringService)
        {
            _repostedMessagesMonitoringService = repostedMessagesMonitoringService;
        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(RemoveDeletedMessages, default, TimerDelay, TimerPeriod);
            return Task.CompletedTask;
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }


        public void Dispose()
        {
            _timer?.Dispose();
        }


        private async void RemoveDeletedMessages(object? _)
        {
            _timer?.Change(Timeout.Infinite, 0);

            await _repostedMessagesMonitoringService.RemoveDeletedMessagesFromChannel();

            _timer?.Change(TimerDelay, TimerPeriod);
        }


        private static readonly TimeSpan TimerDelay = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan TimerPeriod = TimeSpan.FromMinutes(1);


        private readonly IRepostedMessagesMonitoringService _repostedMessagesMonitoringService;
        private Timer? _timer;
    }
}