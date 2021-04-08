using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using HrBot.Models;

namespace HrBot.Services
{
    public class RepostedMessagesStorage : IRepostedMessagesStorage
    {
        public void Add(MessageInfo from, MessageInfo to, DateTimeOffset when)
        {
            _storage.TryAdd(from, new RepostedMessageInfo(from, to, when));
        }


        public List<RepostedMessageInfo> Get()
        {
            var toDelete = _storage.Values
                .Where(x => x.When < DateTimeOffset.Now.Add(-_storageLimit)).ToList();
            foreach (var message in toDelete)
                _storage.TryRemove(message.From, out _);

            return _storage.Values.ToList();
        }


        public void Remove(RepostedMessageInfo message)
        {
            _storage.TryRemove(message.From, out _);
        }


        private readonly ConcurrentDictionary<MessageInfo, RepostedMessageInfo> _storage = new();
        private readonly TimeSpan _storageLimit = TimeSpan.FromHours(2);
    }
}
