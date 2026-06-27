using DSharpPlus.Entities;
using MurdoxV2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Services.Builders.Profile
{
    public class MemberProfileDTO
    {
        public required ulong MemberId { get; set; }
        public required ulong GuildId  { get; set; }
        public string? NickName { get; set; }
        public string? GlobalUsername { get; set; }
        public string? GuildTag { get; set; }
        public string? MemberAvatarUrl { get; set; }
        public string? UserStatus { get; set; }
        public string? VerifiedStatus { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? JoinedGuildAt { get; set; }
        public List<DiscordRole>? Roles { get; set; }
        public int MessageCount { get; set; }
        public int XP { get; set; }

        //==========================//
        //INCLUDES
        //=========================//
        public Bank? Bank { get; set; }
        public List<Ticket>? Tickets { get; set; }
        public List<Reminder>? Reminders { get; set; }
    }
}
