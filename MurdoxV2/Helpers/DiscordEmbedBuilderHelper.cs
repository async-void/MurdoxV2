using DSharpPlus.Entities;
using MurdoxV2.Features.ScamDetection;
using MurdoxV2.Models;
using System.Text;

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

        #region BUILD LOGGER EMBED

        public async Task<DiscordContainerComponent> BuildScamImageLogContainer(ScamImageContext ctx)
        {
            var btns = new DiscordButtonComponent[]
            {
                new(DiscordButtonStyle.Primary, "scamAccept", "Accept", false),
                new(DiscordButtonStyle.Danger, "scamReject", "Reject", false)
            };

            DiscordComponent[] comps =
            [
                new DiscordTextDisplayComponent("## Scam Image Dectected"),
                new DiscordSeparatorComponent(true),
                new DiscordTextDisplayComponent($"Guild: {ctx.Guild.Name}\r\nChannel: {ctx.Channel.Name}\r\nTimestamp: {ctx.Message.Timestamp}"),
                new DiscordMediaGalleryComponent(new DiscordMediaGalleryItem(ctx.Attachment!.Url!, "scam image", false)),
                new DiscordSeparatorComponent(true, DiscordSeparatorSpacing.Large),
                new DiscordActionRowComponent(btns)
            ];
            var container = new DiscordContainerComponent(comps, false, DiscordColor.DarkGray);

            return container;
        }

        #endregion

    }
}
