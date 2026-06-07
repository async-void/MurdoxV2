using MurdoxV2.Enums;
using System.ComponentModel.DataAnnotations;

namespace MurdoxV2.Models
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }
        public ulong TicketId { get; set; }
        public required ulong UserId { get; set; }
        public required ulong GuildId { get; set; }
        public ulong ThreadId { get; set; }
        public required string Content { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ClosedAt { get; set; }
        public TicketStatus Status { get; set; }
        public TicketType Type { get; set; }
        public List<string>? ChatLog { get; set; } = [];
    }
}
