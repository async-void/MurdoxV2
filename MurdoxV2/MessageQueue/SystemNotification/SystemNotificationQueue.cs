using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.MessageQueue.SystemNotification
{
    public sealed class SystemNotificationQueue: ISystemNotificationQueue
    {
        private readonly ConcurrentQueue<SystemNotificationPayload> _queue = new();
        public void Enqueue(SystemNotificationPayload payload) => _queue.Enqueue(payload);
        public bool TryDequeue(out SystemNotificationPayload? payload) => _queue.TryDequeue(out payload);
    }
}
