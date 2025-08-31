﻿using System;
using System.Threading.Tasks;
using HrBot.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace HrBot.Services
{
    public class RepostedMessagesMonitoringService : IRepostedMessagesMonitoringService
    {
        private readonly AppSettings _settings;
        private readonly IRepostedMessagesStorage _storage;
        private readonly ITelegramBotClient _telegram;
        private readonly ILogger<RepostedMessagesMonitoringService> _logger;

        public RepostedMessagesMonitoringService(
            IRepostedMessagesStorage storage,
            IOptions<AppSettings> settings,
            ITelegramBotClient telegram,
            ILogger<RepostedMessagesMonitoringService> logger)
        {
            _storage = storage;
            _settings = settings.Value;
            _telegram = telegram;
            _logger = logger;
        }

        public async Task RemoveDeletedMessages()
        {
            var repostedMessages = _storage.GetAll();

            foreach (var repostedMessage in repostedMessages)
            {
                // Is this correct? The method starts on timer already, also greater number of messages lead to slower response. Potential slow-down point
                await Task.Delay(1000);
                var isDeleted = await IsRepostedMessageDeletedInChat(repostedMessage);

                if (!isDeleted)
                {
                    continue;
                }

                await DeleteRepostedMessageFromChannel(repostedMessage);
                _storage.Remove(repostedMessage);

                _logger.LogInformation(
                    "Message from {ChatId} {MessageId} was deleted",
                    repostedMessage.From.ChatId,
                    repostedMessage.From.MessageId);
            }
        }

        private async Task DeleteRepostedMessageFromChannel(RepostedMessage repostedMessage)
        {
            var channelMessage = repostedMessage.To;
            
            await _telegram.DeleteMessage(channelMessage.ChatId, channelMessage.MessageId);
        }

        private async Task<bool> IsRepostedMessageDeletedInChat(RepostedMessage repostedMessage)
        {
            // Again, can't change it without debugging, probably checking message existence is enough 
            var technicalChatId = _settings.TechnicalChatId;
            try
            {
                var forwarded = await _telegram.ForwardMessage(
                    technicalChatId,
                    repostedMessage.From.ChatId,
                    repostedMessage.From.MessageId,
                    disableNotification: true);
                await _telegram.DeleteMessage(forwarded.Chat.Id, forwarded.MessageId);

                return false;
            }
            catch (Exception exception) when(exception.Message == "Bad Request: message to forward not found")
            {
                var e = exception;
            }

            return true;
        }
    }
}