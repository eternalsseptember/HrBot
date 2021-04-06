using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types;

namespace HrBot.Services
{
    public class VacancyAnalyzer : IVacancyAnalyzer
    {
        public VacancyAnalyzer(IMessageAnalyzer messageAnalyzer)
        {
            _messageAnalyzer = messageAnalyzer;
        }


        public string GetTagsMissingWarningMessage(Message message)
        {
            var missingTags = new List<string>(2);
            var tags = _messageAnalyzer.GetTags(message);

            if (!tags.Any(x => _placeOfWorkTag.Contains(x)))
                missingTags.Add("#удалёнка или #офис");
            
            if (!tags.Any(x => _employmentTypeTag.Contains(x)))
                missingTags.Add("#parttime или #fulltime");

            if (!missingTags.Any())
                return string.Empty;

            return "Здравствуйте! Кажется, вы прислали вакансию. " +
                "Согласно правилам нужно также указать следующие теги: \r\n" +
                $"{string.Join("\r\n", missingTags.Select(x => x))}" +
                "\r\n\r\nНе забудьте указать вилку: зарплатные ожидания от и до.";
        }


        public bool HasMissingTags(Message message)
        {
            var tags = _messageAnalyzer.GetTags(message);
            
            if (!tags.Any(x => _placeOfWorkTag.Contains(x)))
                return true;
            
            return !tags.Any(x => _employmentTypeTag.Contains(x));
        }

        
        private readonly string[] _employmentTypeTag = {"#parttime", "#fulltime"};
        private readonly string[] _placeOfWorkTag = {"#удаленка", "#удалёнка", "#офис"};
        
        
        private readonly IMessageAnalyzer _messageAnalyzer;
    }
}