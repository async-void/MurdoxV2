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
        public required string GuildId { get; set; }
        public required string GuildName { get; set; }
        public required string OwnerId { get; set; }
        public required string OwnerUsername { get; set; }
        public required string NotificationChannelId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public bool EnableFacts { get; set; }
        public ICollection<ServerMember>? Members { get; set; }
    }
}
