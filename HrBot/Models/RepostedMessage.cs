using System;
using HrBot.Services;

namespace HrBot.Models
{
    public record RepostedMessage(ChatMessageId From, ChatMessageId To, DateTimeOffset When);
}