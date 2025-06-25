using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Models
{
    public class Reminder
    {
        public int Id { get; set; }
        public required string Content { get; set; }
        public required string GuildId { get; set; }
        public required string ChannelId { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime CompleteAt { get; set; }

        public int MemberId { get; set; }
        public ServerMember? Member { get; set; }

    }
}
