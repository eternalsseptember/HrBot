using System.Collections.Generic;
using HrBot.Models;
using Telegram.Bot.Types;

namespace HrBot.Services
{
    public interface IMessageAnalyzer
    {
        List<string> GetTags(Message message);
        MessageTypes GetType(Message message);
    }
}
