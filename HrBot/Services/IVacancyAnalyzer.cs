using System.Collections.Generic;
using HrBot.Models;
using Telegram.Bot.Types;

namespace HrBot.Services
{
    public interface IVacancyAnalyzer
    {
        bool IsVacancy(Message message);

        bool IsResume(Message message);

        IEnumerable<VacancyError> GetVacancyErrors(Message message);
    }
}
