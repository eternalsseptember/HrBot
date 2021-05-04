using System.Collections.Generic;

namespace HrBot.Configuration
{
    public class ChatOptions
    {
        public HashSet<long> AllowedChatIds { get; set; } = new();
        public long ChannelToRepostId { get; set; }
        public bool RepostOnlyFromAllowedChats { get; set; } = false;
        public long TechnicalChatId { get; set; } 
    }
}