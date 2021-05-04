using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HrBot.Configuration;
using HrBot.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace HrBot.Services
{
    public class VacancyReposter : IVacancyReposter
    {
        private static string GetErrorKey(Message message) => $"ErrorMessage_{message.Chat.Id}_{message.MessageId}";
        
        private static string GetMessageKey(Message message) => $"RepostedMessage_{message.Chat.Id}_{message.MessageId}";
        
        private readonly IMemoryCache _memoryCache;
        private readonly IMessageAnalyzer _messageAnalyzer;
        private readonly IRepostedMessagesStorage _repostedMessagesStorage;
        private readonly ChatOptions _options;
        private readonly ITelegramBotClient _telegramBot;
        private readonly IVacancyAnalyzer _vacancyAnalyzer;


        public VacancyReposter(
            IMemoryCache memoryCache,
            IOptions<ChatOptions> options,
            ITelegramBotClient telegramBot,
            IMessageAnalyzer messageAnalyzer,
            IRepostedMessagesStorage repostedMessagesStorage,
            IVacancyAnalyzer vacancyAnalyzer)
        {
            _memoryCache = memoryCache;
            _options = options.Value;
            _telegramBot = telegramBot;
            _messageAnalyzer = messageAnalyzer;
            _repostedMessagesStorage = repostedMessagesStorage;
            _vacancyAnalyzer = vacancyAnalyzer;
        }


        public async Task Edit(Message message)
        {
            var messageType = _messageAnalyzer.GetType(message);
            if (messageType == MessageTypes.Chat)
                return;

            if (messageType == MessageTypes.Vacancy)
            {
                if (!_vacancyAnalyzer.HasMissingTags(message))
                    await DeleteTagsMissingWarningIfExists(message);
            }

            if (_memoryCache.TryGetValue(GetMessageKey(message), out MessageInfo repostedMessageIds))
                await _telegramBot.EditMessageTextAsync(repostedMessageIds.ChatId, repostedMessageIds.MessageId, GetMessageWithAuthor(message), ParseMode.Html);
        }


        public async Task RepostToChannel(Message message)
        {
            var messageType = _messageAnalyzer.GetType(message);
            if (messageType == MessageTypes.Chat)
                return;

            if (messageType == MessageTypes.Vacancy)
            {
                if (_vacancyAnalyzer.HasMissingTags(message))
                    await SendTagsMissingWarning(message);
            }

            await RepostMessage(message);
        }


        private async Task DeleteTagsMissingWarningIfExists(Message message)
        {
            if (!_memoryCache.TryGetValue(GetErrorKey(message), out MessageInfo messageInfo))
                return;

            await _telegramBot.DeleteMessageAsync(messageInfo.ChatId, messageInfo.MessageId);
        }


        private async Task RepostMessage(Message message)
        {
            var repostedMessage = await _telegramBot.SendTextMessageAsync(_options.ChannelToRepostId, GetMessageWithAuthor(message), ParseMode.Html);

            _memoryCache.Set(GetMessageKey(message), new MessageInfo(repostedMessage.Chat.Id, repostedMessage.MessageId));

            _repostedMessagesStorage.Add(new MessageInfo(message.Chat.Id, message.MessageId),
                new MessageInfo(repostedMessage.Chat.Id, repostedMessage.MessageId), DateTimeOffset.Now);
        }


        private async Task SendTagsMissingWarning(Message message)
        {
            var tagsMissingWarning = _vacancyAnalyzer.GetTagsMissingWarningMessage(message);
            if (tagsMissingWarning == string.Empty)
                return;

            var warningMessage = await _telegramBot.SendTextMessageAsync(message.Chat.Id, tagsMissingWarning, replyToMessageId: message.MessageId);

            _memoryCache.Set(GetErrorKey(message), new MessageInfo(warningMessage.Chat.Id, warningMessage.MessageId));
        }


        private static string GetMessageWithAuthor(Message message)
        {
            var authorId = message.From.Id;
            var newMessageWithAuthor =
                $"{message.Text}\n\n<a href=\"tg://user?id={authorId}\">{GetPrettyName(message.From)}</a>";

            return newMessageWithAuthor;
        }


        private static string GetPrettyName(User user)
        {
            var names = new List<string>(3);

            if (!string.IsNullOrWhiteSpace(user.FirstName))
                names.Add(user.FirstName);

            if (!string.IsNullOrWhiteSpace(user.LastName))
                names.Add(user.LastName);

            if (!string.IsNullOrWhiteSpace(user.Username))
                names.Add($"(@{user.Username})");

            return string.Join(" ", names);
        }
    }
}
