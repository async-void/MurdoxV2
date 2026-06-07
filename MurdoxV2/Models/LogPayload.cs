using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Models
{
    public class LogPayload
    {
        public required ulong GuildId { get; set; }
        public required ulong ChannelId { get; set; }
        public required string Message { get; set; }
    }
}
