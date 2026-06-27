using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Models
{
    public class CachedDiscordMessage
    {
        [Key]
        public int Id { get; set; }
        public ulong MessageId { get; set; }
        public ulong AuthorId { get; set; }
        public ulong ChannelId { get; set; }
        public required string Content { get; set; }
        public DateTimeOffset? LastMessageTimestamp { get; set; }
        public IReadOnlyList<ulong> MentionedUserIds { get; set; } = [];
    }
}
