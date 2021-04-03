using System;
using System.Collections.Generic;
using System.Linq;
using HrBot.Models;
using Telegram.Bot.Types;

namespace HrBot.Services
{
    internal class VacancyAnalyzer : IVacancyAnalyzer
    {
        private const string VacancyTag = "#вакансия";
        private const string ResumeTag = "#резюме";
        private readonly string[] _placeOfWorkTag = new[] {
            "#удаленка", "#удалёнка", "#офис"
        };
        private readonly string[] _employmentTypeTag = new[] {
            "#parttime", "#fulltime"
        };


        public MessageTypes GetMessageType(Message message)
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


        public IEnumerable<VacancyError> GetVacancyErrors(Message message)
        {
            var tags = GetTags(message);

            if (!tags.Any(x => _placeOfWorkTag.Contains(x)))
            {
                yield return new VacancyError(ErrorType.WithoutTag, "#удалёнка или #офис");
            }
            if (!tags.Any(x => _employmentTypeTag.Contains(x)))
            {
                yield return new VacancyError(ErrorType.WithoutTag, "#parttime или #fulltime");
            }
        }


        private static List<string> GetTags(Message message)
        {
            if (message.EntityValues is null)
                return new List<string>(0);

            return message.EntityValues
                .Where(x => x.StartsWith("#"))
                .Select(x => x.ToLowerInvariant())
                .ToList();
        }
    }
}