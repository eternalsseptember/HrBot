using System;

namespace HrBot.Models
{
    public record RepostedMessageInfo(MessageInfo From, MessageInfo To, DateTimeOffset When);
}