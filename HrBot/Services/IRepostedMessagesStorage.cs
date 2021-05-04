using System;
using System.Collections.Generic;
using HrBot.Models;

namespace HrBot.Services
{
    public interface IRepostedMessagesStorage
    {
        void Add(MessageInfo from, MessageInfo to, DateTimeOffset when);

        IReadOnlyCollection<RepostedMessageInfo> Get();

        void Remove(RepostedMessageInfo repostedMessage);
    }
}