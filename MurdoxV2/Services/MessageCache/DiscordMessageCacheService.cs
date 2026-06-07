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

        public int Count => _msgCache.Count;
        public void Set(CachedDiscordMessage msg) =>
            _msgCache[msg.MessageId] = msg;

        public bool TryGet(ulong messageId, out CachedDiscordMessage? cached) =>
            _msgCache.TryGetValue(messageId, out cached);

        public void Remove(ulong msgId) =>
            _msgCache.TryRemove(msgId, out _);

        public void Clear() => _msgCache.Clear();
    }
}
