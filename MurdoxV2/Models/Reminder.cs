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
        public required string Title { get; set; }
        public required string Content { get; set; }
        public required ulong GuildId { get; set; }
        public required ulong ChannelId { get; set; }
        public required ulong DiscordId { get; set; }
        public required TimeSpan Duration { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset CompleteAt { get; set; }
        public bool IsComplete { get; set; }

        //Navigation Properties
        public int ServerMemberId { get; set; }
        public ServerMember Member { get; set; }
    }
}
