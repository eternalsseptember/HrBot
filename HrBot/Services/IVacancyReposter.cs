using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace HrBot.Services
{
    public interface IVacancyReposter
    {
        Task Edit(Message message);
        Task RepostToChannel(Message message);
    }
}