using System.Collections.Generic;
using System.Linq;
using HrBot.Models;
using Telegram.Bot.Types;

namespace HrBot.Services
{
    public class MessageAnalyzer : IMessageAnalyzer
    {
        public List<string> GetTags(Message message)
        {
            if (message.EntityValues is null)
                return new List<string>(0);

            return message.EntityValues
                .Where(x => x.StartsWith("#"))
                .Select(x => x.ToLowerInvariant())
                .ToList();
        }


        public MessageTypes GetType(Message message)
        {
            var tags = GetTags(message);
            foreach (var tag in tags)
                switch (tag)
                {
                    case VacancyTag:
                        return MessageTypes.Vacancy;
                    case ResumeTag:
                        return MessageTypes.Resume;
                }

            return MessageTypes.Chat;
        }


        private const string ResumeTag = "#резюме";
        private const string VacancyTag = "#вакансия";
    }
}
