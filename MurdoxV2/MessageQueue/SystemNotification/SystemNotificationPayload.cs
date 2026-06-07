using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.MessageQueue.SystemNotification
{
    public sealed class SystemNotificationPayload
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public ulong GuildId { get; set; }
        public ulong Author { get; set; }
        public required string Message { get; set; }
        public DateTimeOffset EnqueuedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
