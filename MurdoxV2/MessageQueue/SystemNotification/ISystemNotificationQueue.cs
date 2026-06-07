using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.MessageQueue.SystemNotification
{
    public interface ISystemNotificationQueue
    {
        void Enqueue(SystemNotificationPayload payload);
        bool TryDequeue(out SystemNotificationPayload? payload);
    }
}
