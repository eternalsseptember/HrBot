using System;
using System.Threading.Tasks;
using HrBot.Configuration;
using HrBot.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace HrBot.Services
{
    public class RepostedMessagesMonitoringService : IRepostedMessagesMonitoringService
    {
        public RepostedMessagesMonitoringService(
            ILogger<RepostedMessagesMonitoringService> logger,
            IOptions<ChatOptions> options,
            ITelegramBotClient telegram,
            IRepostedMessagesStorage storage)
        {
            _logger = logger;
            _options = options.Value;
            _storage = storage;
            _telegram = telegram;
        }


        public async Task RemoveDeletedMessagesFromChannel()
        {
            try
            {
                var repostedMessages = _storage.Get();

                foreach (var repostedMessage in repostedMessages)
                {
                    // https://core.telegram.org/bots/faq#my-bot-is-hitting-limits-how-do-i-avoid-this
                    // According the documentation a bot allows to make about 20 requests per minute
                    await Task.Delay(3000);

                    var isDeleted = await IsDeletedFromChat(repostedMessage);
                    if (!isDeleted)
                        continue;

                    await RemoveRepostedMessageFromChannel(repostedMessage);
                    _storage.Remove(repostedMessage);

                    _logger.LogInformation("Message from {ChatId} {MessageId} was deleted", repostedMessage.From.ChatId, repostedMessage.From.MessageId);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An exception is occurred during message monitoring: {Message}", e.Message);
            }
        }


        private async Task RemoveRepostedMessageFromChannel(RepostedMessageInfo repostedMessage)
        {
            var channeledMessageInfo = repostedMessage.To;
            await _telegram.DeleteMessageAsync(channeledMessageInfo.ChatId, channeledMessageInfo.MessageId);
        }


        private async Task<bool> IsDeletedFromChat(RepostedMessageInfo repostedMessage)
        {
            // Again, can't change it without debugging, probably checking message existence is enough 
            try
            {
                var forwarded = await _telegram.ForwardMessageAsync(_options.TechnicalChatId, repostedMessage.From.ChatId, repostedMessage.From.MessageId,
                    disableNotification: true);
                await _telegram.DeleteMessageAsync(forwarded.Chat.Id, forwarded.MessageId);

                return false;
            }
            catch (Exception exception) when (exception.Message == "Bad Request: message to forward not found")
            {
                // Swallow the exception when a message has deleted already 
            }

            return true;
        }


        private readonly ILogger<RepostedMessagesMonitoringService> _logger;
        private readonly ChatOptions _options;
        private readonly IRepostedMessagesStorage _storage;
        private readonly ITelegramBotClient _telegram;
    }
}