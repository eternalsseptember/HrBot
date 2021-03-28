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

        public bool IsVacancy(Message message)
        {
            var tags = GetTags(message);

            return tags.Any(x => x == VacancyTag);
        }

        public bool IsResume(Message message)
        {
            var tags = GetTags(message);

            return tags.Any(x => x == ResumeTag);
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

        private static IReadOnlyCollection<string> GetTags(Message message)
        {
            return message.EntityValues?
                .Where(x => x.StartsWith("#"))
                .Select(x => x.ToLowerInvariant())
                .ToArray()
                ?? Array.Empty<string>();
        }
    }
}