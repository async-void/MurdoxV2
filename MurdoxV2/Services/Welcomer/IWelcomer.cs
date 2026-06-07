using DSharpPlus.Entities;
using MurdoxV2.Services.Welcomer.Guild;
using MurdoxV2.Services.Welcomer.Member;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Services.Welcomer
{
    public interface IWelcomer
    {
        Task<GuildWelcomeMessage> GetGuildWelcomeAsync(DiscordGuild guild);
        Task<MemberWelcomeMessage> GetMemberWelcomeAsync(DiscordMember member);
    }
}
