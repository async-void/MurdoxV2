using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Models
{
    public class Server
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public required string GuildName { get; set; }
        public ulong OwnerId { get; set; }
        public required string OwnerUsername { get; set; }
        public ulong NotificationChannelId { get; set; }
        public ulong HoneypotChannelId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public bool EnableFacts { get; set; }
        public bool AllowUrls { get; set; }
        public ICollection<ServerMember>? Members { get; set; }
    }
}
