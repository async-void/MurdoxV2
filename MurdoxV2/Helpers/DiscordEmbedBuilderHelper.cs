using DSharpPlus.Entities;
using MurdoxV2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Helpers
{
    public class DiscordEmbedBuilderHelper
    {
        #region BUILD REMINDER EMBED
        public DiscordEmbed BuildReminderEmbed(IEnumerable<Reminder> reminders, int page, int totalPages)
        {
            var embed = new DiscordEmbedBuilder()
                .WithTitle($"⏰ Your Reminders (Page {page + 1}/{totalPages})")
                .WithColor(DiscordColor.Azure);

            foreach (var reminder in reminders)
            {
                embed.AddField($"{reminder.Id}.", reminder.Content, true);
            }

            return embed;
        }
        #endregion

        #region BUILD WHOIS EMBED
        public async Task<Result<StringBuilder, SystemError<DiscordEmbedBuilderHelper>>> BuildWhoisDetailsAsync(ServerMember member, DiscordUser user)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"**Username:** {member.GlobalUsername}");
            sb.AppendLine($"**ID:** {member.Id}");
            sb.AppendLine($"**Joined At:** {user.CreationTimestamp.ToString("f") ?? "N/A"}");
            sb.AppendLine($"**Avatar URL:** [Click Here]({user.AvatarUrl})");
            return Result<StringBuilder, SystemError<DiscordEmbedBuilderHelper>>.Ok(sb);
        }
        #endregion

    }
}
