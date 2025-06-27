using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Models
{
    public class ServerMember
    {
        public int Id { get; set; }
        public required string DiscordId { get; set; }
        public required string GuildId { get; set; }
        public required string GlobalUsername{ get; set; }
        public required string Discriminator { get; set; }
        public required string Nickname { get; set; }
        public required string AvatarUrl { get; set; }
        public string? UserStatus { get; set; }
        public int XP { get; set; }
        public DateTimeOffset? JoinedAt { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public bool? IsBot { get; set; }
        public bool? IsMuted { get; set; }
        public bool? IsBanned { get; set; }
        public int MessageCount { get; set; }

        public int BankId { get; set; } //Foreign Key
        public Bank? Bank { get; set; } //Navigation Property One Bank to Many ServerMembers
        public ICollection<Reminder>? Reminders { get; set; }

    }
}
