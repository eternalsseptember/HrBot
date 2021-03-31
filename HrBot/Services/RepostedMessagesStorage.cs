using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using HrBot.Models;

namespace HrBot.Services
{
    public class RepostedMessagesStorage : IRepostedMessagesStorage
    {
        private readonly ConcurrentDictionary<ChatMessageId, RepostedMessage> _storage = new();
        private readonly TimeSpan _storageLimit = TimeSpan.FromHours(2);

        public IReadOnlyCollection<RepostedMessage> GetAll()
        {
            var toDelete = _storage.Values.Where(x => x.When < DateTimeOffset.Now.Add(-_storageLimit)).ToList();
            foreach (var message in toDelete)
            {
                _storage.TryRemove(message.From, out var _);
            }

            return _storage.Values.ToList();
        }

        public void Add(ChatMessageId from, ChatMessageId to, DateTimeOffset when)
        {
            _storage.TryAdd(from, new RepostedMessage(from, to, when));
        }

        public void Remove(RepostedMessage message)
        {
            _storage.TryRemove(message.From, out var _);
        }
    }
}
