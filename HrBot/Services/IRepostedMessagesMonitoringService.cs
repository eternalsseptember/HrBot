using System.Threading.Tasks;

namespace HrBot.Services
{
    public interface IRepostedMessagesMonitoringService
    {
        Task RemoveDeletedMessagesFromChannel();
    }
}