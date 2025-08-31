using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IMemoryCache _memoryCache;
        private readonly AppSettings _settings;
        private readonly ITelegramBotClient _telegramBot;
        private readonly IVacancyAnalyzer _vacancyAnalyzer;
        private readonly IRepostedMessagesStorage _repostedMessagesStorage;

        public VacancyReposter(
            IVacancyAnalyzer vacancyAnalyzer,
            ITelegramBotClient telegramBot,
            IOptions<AppSettings> settings,
            IMemoryCache memoryCache,
            IRepostedMessagesStorage repostedMessagesStorage)
        {
            _vacancyAnalyzer = vacancyAnalyzer;
            _telegramBot = telegramBot;
            _settings = settings.Value;
            _memoryCache = memoryCache;
            _repostedMessagesStorage = repostedMessagesStorage;
        }

        public async Task TryRepost(Message message)
        {
            var messageType = _vacancyAnalyzer.GetMessageType(message);
            if (messageType == MessageTypes.Chat)
                return;

            if (messageType == MessageTypes.Vacancy)
            {
                if (_vacancyAnalyzer.HasMissingTags(message))
                    await SendMissingTagsWarning(message);
            }

            var repostedMessage = await _telegramBot.SendMessage(
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

        public async Task TryEdit(Message message)
        {
            var messageType = _vacancyAnalyzer.GetMessageType(message);
            if (messageType == MessageTypes.Chat)
                return;

            if (messageType == MessageTypes.Vacancy)
            {
                if (!_vacancyAnalyzer.HasMissingTags(message))
                    await TryDeleteMissingTagsWarning(message);
            }

            if (_memoryCache.TryGetValue(GetKey(message), out ChatMessageId? data)
                && data is {} repostedMessageIds)
            {
                await _telegramBot.EditMessageText(
                    repostedMessageIds.ChatId,
                    repostedMessageIds.MessageId,
                    GetMessageWithAuthor(message),
                    ParseMode.Html);
            }
        }

        private async Task TryDeleteMissingTagsWarning(Message message)
        {
            if (_memoryCache.TryGetValue(GetErrorKey(message), out ChatMessageId? data)
                && data is { } warningMessageIds)
            {
                await _telegramBot.DeleteMessage(
                    warningMessageIds.ChatId,
                    warningMessageIds.MessageId);
            }
        }

        private async Task SendMissingTagsWarning(Message message)
        {
            var missingTagsKinds = _vacancyAnalyzer.GetVacancyErrors(message);
            var errorText =
                "Здравствуйте! Кажется, вы прислали вакансию. " +
                "Согласно правилам нужно также указать следующие теги: \r\n" +
                $"{string.Join("\r\n", missingTagsKinds.Select(x => x))}" +
                "\r\n\r\nНе забудьте указать вилку: зарплатные ожидания от и до.";

            var errorMessage = await _telegramBot.SendMessage(
                message.Chat.Id,
                errorText,
                replyParameters: new ReplyParameters { MessageId = message.MessageId });

            _memoryCache.Set(
                GetErrorKey(message),
                new ChatMessageId(errorMessage.Chat.Id, errorMessage.MessageId));
        }

        private static string GetMessageWithAuthor(Message message)
        {
            var authorId = message.From?.Id;
            var newMessageWithAuthor =
                $"{message.Text}\n\n<a href=\"tg://user?id={authorId}\">{GetPrettyName(message.From)}</a>";
            return newMessageWithAuthor;
        }

        private static string GetKey(Message message) => $"RepostedMessage_{message.Chat.Id}_{message.MessageId}";

        private static string GetErrorKey(Message message) => $"ErrorMessage_{message.Chat.Id}_{message.MessageId}";

        private static string GetPrettyName(User? user)
        {
            if (user == null) return "Unknown";

            var names = new List<string>(3);

            if (!string.IsNullOrWhiteSpace(user.FirstName))
                names.Add(user.FirstName);
            if (!string.IsNullOrWhiteSpace(user.LastName))
                names.Add(user.LastName);
            if (!string.IsNullOrWhiteSpace(user.Username))
                names.Add("(@" + user.Username + ")");

            return string.Join(" ", names);
        }
    }
}
