using System;
using System.Collections.Generic;
using HrBot.Models;

namespace HrBot.Services
{
    public interface IRepostedMessagesStorage
    {
        IReadOnlyCollection<RepostedMessage> GetAll();

        void Add(ChatMessageId from, ChatMessageId to, DateTimeOffset when);

        void Remove(RepostedMessage repostedMessage);
    }
}