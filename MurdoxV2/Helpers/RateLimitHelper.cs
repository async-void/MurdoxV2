using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Helpers
{
    public sealed class RateLimitHelper<TKey>(TimeSpan cooldown) where TKey: notnull
    {
        private readonly ConcurrentDictionary<TKey, SemaphoreSlim> _userLocks = new();
        private readonly ConcurrentDictionary<TKey, DateTimeOffset> _nextAvailable = new();
        private readonly TimeSpan Cooldown = cooldown;

        public async Task WaitForRateLimitAsync(TKey key)
        {
            var sem = _userLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
            await sem.WaitAsync();

            try
            {
                var now = DateTimeOffset.UtcNow;

                if (_nextAvailable.TryGetValue(key, out var next))
                {
                    if (next > now)
                    {
                        var delay = next - now;
                        await Task.Delay(delay);
                    }
                }

                _nextAvailable[key] = DateTimeOffset.UtcNow + Cooldown;
            }
            finally
            {
                sem.Release();
            }
        }
    }
}
