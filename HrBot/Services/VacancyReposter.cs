using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace HrBot.Services
{
    internal class VacancyReposter : IVacancyReposter
    {
        private readonly IMemoryCache _memoryCache;
        private readonly AppSettings _settings;
        private readonly ITelegramBotClient _telegramBot;
        private readonly IVacancyAnalyzer _vacancyAnalyzer;

        public VacancyReposter(
            IVacancyAnalyzer vacancyAnalyzer,
            ITelegramBotClient telegramBot,
            AppSettings settings,
            IMemoryCache memoryCache)
        {
            _vacancyAnalyzer = vacancyAnalyzer;
            _telegramBot = telegramBot;
            _settings = settings;
            _memoryCache = memoryCache;
        }

        public async Task TryRepost(Message message)
        {
            var isVacancy = _vacancyAnalyzer.IsVacancy(message);
            var isResume = _vacancyAnalyzer.IsResume(message);

            if (!isVacancy && !isResume)
            {
                return;
            }

            if (isVacancy)
            {
                await SendVacancyWarnings(message);
            }

            var repostedMessage = await _telegramBot.SendTextMessageAsync(
                _settings.RepostToChannelId,
                GetMessageWithAuthor(message),
                ParseMode.Html);

            _memoryCache.Set(
                GetKey(message),
                new ChatMessageId(repostedMessage.Chat.Id, repostedMessage.MessageId));
        }

        public async Task TryEdit(Message message)
        {
            var isVacancy = _vacancyAnalyzer.IsVacancy(message);
            var isResume = _vacancyAnalyzer.IsResume(message);

            if (!isVacancy && !isResume)
            {
                return;
            }

            if (isVacancy)
            {
                await TryDeleteWarningMessage(message);
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

        private async Task TryDeleteWarningMessage(Message message)
        {
            var vacancyErrors = _vacancyAnalyzer.GetVacancyErrors(message).ToList();
            if (vacancyErrors.Count != 0)
            {
                return;
            }

            var hasWarningMessage = _memoryCache.TryGetValue(
                GetErrorKey(message),
                out ChatMessageId warningMessageIds);

            if (hasWarningMessage)
            {
                await _telegramBot.DeleteMessageAsync(
                    warningMessageIds.ChatId,
                    warningMessageIds.MessageId);
            }
        }

        private async Task SendVacancyWarnings(Message message)
        {
            var vacancyErrors = _vacancyAnalyzer.GetVacancyErrors(message).ToList();
            if (vacancyErrors.Count > 0)
            {
                var errorMessage =
                    "Здравствуйте! Кажется, вы прислали вакансию. " +
                    "Согласно правилам нужно также указать следующие теги: \r\n" +
                    $"{string.Join("\r\n", vacancyErrors.Select(x => x.Value))}" +
                    "\r\n\r\nНе забудьте указать вилку: зарплатные ожидания от и до.";

                var sentErrorMessage = await _telegramBot.SendTextMessageAsync(
                    message.Chat.Id,
                    errorMessage,
                    replyToMessageId: message.MessageId);

                _memoryCache.Set(
                    GetErrorKey(message),
                    new ChatMessageId(sentErrorMessage.Chat.Id, sentErrorMessage.MessageId));
            }
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
    }
}
