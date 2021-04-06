using System;
using HrBot.Services;

namespace HrBot.Models
{
    public record RepostedMessage(MessageInfo From, MessageInfo To, DateTimeOffset When);
}