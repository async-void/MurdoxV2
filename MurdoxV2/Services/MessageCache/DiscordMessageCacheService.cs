using MurdoxV2.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Services.MessageCache
{
    public sealed class DiscordMessageCacheService
    {
        private readonly ConcurrentDictionary<ulong, CachedDiscordMessage> _msgCache = [];
        private readonly ConcurrentDictionary<ulong, ConcurrentBag<ulong>> _memberMsgCache  = [];

        public int Count => _msgCache.Count;
        public void Set(CachedDiscordMessage msg)
        {
            _msgCache[msg.MessageId] = msg;
            var bag = _memberMsgCache.GetOrAdd(msg.AuthorId, _ => []);
            bag.Add(msg.AuthorId);
        }
            

        public bool TryGet(ulong messageId, out CachedDiscordMessage? cached) =>
            _msgCache.TryGetValue(messageId, out cached);

        public void Remove(ulong msgId) =>
            _msgCache.TryRemove(msgId, out _);

        public void Clear()
        {
            _msgCache.Clear();
            _memberMsgCache.Clear();
        }

        public IReadOnlyList<CachedDiscordMessage> GetMessagesByMember(ulong memberId)
        {
            if (!_memberMsgCache.TryGetValue(memberId, out var bag))
                return [];
            var results = new List<CachedDiscordMessage>();
            foreach (var msgId in bag)
            {
                if (_msgCache.TryGetValue(msgId, out var msg))
                    results.Add(msg);
            }
            return results;
        }
    }
}
