using Telegram.Bot.Types;

namespace HrBot.Services
{
    public interface IVacancyAnalyzer
    {
        string GetTagsMissingWarningMessage(Message message);

        bool HasMissingTags(Message message);
    }
}
