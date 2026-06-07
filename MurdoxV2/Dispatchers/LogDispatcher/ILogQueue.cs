using MurdoxV2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Dispatchers.LogDispatcher
{
    public interface ILogQueue
    {
        void Enqueue(LogPayload logMessage);
        bool TryDequeue(out LogPayload? payload);
        int Count { get; }
    }
}
