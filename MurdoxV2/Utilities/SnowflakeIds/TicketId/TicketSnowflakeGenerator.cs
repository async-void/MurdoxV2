using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Utilities.SnowflakeIds.TicketId
{
    public static class TicketSnowflakeGenerator
    {
        // Custom epoch (Discord uses 2015-01-01)
        private static readonly long _epoch =
            new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();

        private static readonly object _lock = new();

        private static long _lastTimestamp = -1;
        private static long _sequence = 0;

        // 10-bit node id (0–1023). You can hardcode 1.
        private const int NodeId = 1;

        public static ulong NextId()
        {
            lock (_lock)
            {
                long timestamp = CurrentTimestamp();

                if (timestamp < _lastTimestamp)
                {
                    // Clock moved backwards — extremely rare
                    timestamp = _lastTimestamp;
                }

                if (timestamp == _lastTimestamp)
                {
                    _sequence = (_sequence + 1) & 0xFFF; // 12 bits

                    if (_sequence == 0)
                    {
                        // Sequence overflow — wait for next millisecond
                        timestamp = WaitNextMillis(timestamp);
                    }
                }
                else
                {
                    _sequence = 0;
                }

                _lastTimestamp = timestamp;

                ulong id =
                    ((ulong)(timestamp - _epoch) << 22) |
                    ((ulong)NodeId << 12) |
                    (ulong)_sequence;

                return id;
            }
        }

        private static long CurrentTimestamp()
            => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        private static long WaitNextMillis(long current)
        {
            long ts;
            do
            {
                ts = CurrentTimestamp();
            } while (ts <= current);

            return ts;
        }
    }
}
