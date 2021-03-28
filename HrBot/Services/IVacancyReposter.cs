using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace HrBot.Services
{
    public interface IVacancyReposter
    {
        Task TryRepost(Message message);

        Task TryEdit(Message message);
    }
}