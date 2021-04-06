using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        public VacancyReposter(
            IMemoryCache memoryCache,
            IOptions<AppSettings> settings,
            ITelegramBotClient telegramBot, 
            IMessageAnalyzer messageAnalyzer,
            IRepostedMessagesStorage repostedMessagesStorage,
            IVacancyAnalyzer vacancyAnalyzer)
        {
            _memoryCache = memoryCache;
            _settings = settings.Value;
            _telegramBot = telegramBot;
            _messageAnalyzer = messageAnalyzer;
            _repostedMessagesStorage = repostedMessagesStorage;
            _vacancyAnalyzer = vacancyAnalyzer;
        }


        public async Task TryEdit(Message message)
        {
            var messageType = _messageAnalyzer.GetType(message);
            if (messageType == MessageTypes.Chat)
                return;

            if (messageType == MessageTypes.Vacancy)
            {
                if (!_vacancyAnalyzer.HasMissingTags(message))
                    await TryDeleteMissingTagsWarning(message);
            }

            if (_memoryCache.TryGetValue(GetKey(message), out ChatMessageId repostedMessageIds))
            {
                await _telegramBot.EditMessageTextAsync(
                    repostedMessageIds.ChatId,
                    repostedMessageIds.MessageId,
                    GetMessageWithAuthor(message),
                    ParseMode.Html);
            }
        }
        
        public async Task TryRepost(Message message)
        {
            var messageType = _messageAnalyzer.GetType(message);
            if (messageType == MessageTypes.Chat)
                return;

            if (messageType == MessageTypes.Vacancy)
            {
                if (_vacancyAnalyzer.HasMissingTags(message))
                    await SendTagsMissingWarning(message);
            }

            var repostedMessage = await _telegramBot.SendTextMessageAsync(
                _settings.RepostToChannelId,
                GetMessageWithAuthor(message),
                ParseMode.Html);

            _memoryCache.Set(
                GetKey(message),
                new ChatMessageId(repostedMessage.Chat.Id, repostedMessage.MessageId));

            _repostedMessagesStorage.Add(
                new ChatMessageId(message.Chat.Id, message.MessageId),
                new ChatMessageId(repostedMessage.Chat.Id, repostedMessage.MessageId),
                DateTimeOffset.Now);
        }


        private async Task TryDeleteMissingTagsWarning(Message message)
        {
            var hasWarningMessage = _memoryCache.TryGetValue(
                GetErrorKey(message),
                out ChatMessageId warningMessageIds);

            if (!hasWarningMessage)
                return;

            await _telegramBot.DeleteMessageAsync(
                warningMessageIds.ChatId,
                warningMessageIds.MessageId);
        }


        private async Task SendTagsMissingWarning(Message message)
        {
            var tagsMissingWarning = _vacancyAnalyzer.GetTagsMissingWarningMessage(message);
            if (tagsMissingWarning == string.Empty)
                return;

            var warningMessage = await _telegramBot.SendTextMessageAsync(message.Chat.Id, tagsMissingWarning, replyToMessageId: message.MessageId);

            _memoryCache.Set(GetErrorKey(message), new ChatMessageId(warningMessage.Chat.Id, warningMessage.MessageId));
        }


        private static string GetMessageWithAuthor(Message message)
        {
            var authorId = message.From.Id;
            var newMessageWithAuthor =
                $"{message.Text}\n\n<a href=\"tg://user?id={authorId}\">{GetPrettyName(message.From)}</a>";
            return newMessageWithAuthor;
        }

        private static string GetKey(Message message) => $"RepostedMessage_{message.Chat.Id}_{message.MessageId}";

        private static string GetErrorKey(Message message) => $"ErrorMessage_{message.Chat.Id}_{message.MessageId}";

        private static string GetPrettyName(User user)
        {
            var names = new List<string>(3);

            if (!string.IsNullOrWhiteSpace(user.FirstName))
                names.Add(user.FirstName);
            if (!string.IsNullOrWhiteSpace(user.LastName))
                names.Add(user.LastName);
            if (!string.IsNullOrWhiteSpace(user.Username))
                names.Add("(@" + user.Username + ")");

            return string.Join(" ", names);
        }


        private readonly IMemoryCache _memoryCache;
        private readonly IMessageAnalyzer _messageAnalyzer;
        private readonly IRepostedMessagesStorage _repostedMessagesStorage;
        private readonly AppSettings _settings;
        private readonly ITelegramBotClient _telegramBot;
        private readonly IVacancyAnalyzer _vacancyAnalyzer;
    }
}
