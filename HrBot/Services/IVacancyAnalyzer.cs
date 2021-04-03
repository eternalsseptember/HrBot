using System.Collections.Generic;
using HrBot.Models;
using Telegram.Bot.Types;

namespace HrBot.Services
{
    public interface IVacancyAnalyzer
    {
        MessageTypes GetMessageType(Message message);

        IEnumerable<VacancyError> GetVacancyErrors(Message message);
    }
}
