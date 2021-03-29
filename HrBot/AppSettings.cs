using System.Collections.Generic;

namespace HrBot
{
    public class AppSettings
    {
        public string BotToken { get; set; } = default!;

        public string WebHookAddress { get; set; } = default!;

        public long RepostToChannelId { get; set; }

        public IReadOnlyCollection<long> RepostOnlyFromChatIds { get; set; } = default!;
    }
}