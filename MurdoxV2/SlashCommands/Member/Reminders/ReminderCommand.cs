using DSharpPlus.Commands;
using DSharpPlus.Entities;
using MurdoxV2.Interfaces;
using MurdoxV2.Models;
using System.ComponentModel;

namespace MurdoxV2.SlashCommands.Member.Reminders
{
    [Command("reminder")]
    [Description("Manage member reminders.")]
    public class ReminderCommand(IReminderData reminderData)
    {
        [Command("remind")]
        [Description("Set a reminder")]
        public async Task SetReminderAsync(CommandContext ctx,
            [Description("The time for the reminder in a format like '1h 30m' or '2d 3h'.")] string time,
            [Description("The message for the reminder.")] string message)
        {
            await ctx.DeferResponseAsync();
            var duration = reminderData.ParseTimeString(time);
            var member = new ServerMember()
            {
                GuildId = ctx.Guild!.Id.ToString(),
                DiscordId = ctx.User.Id.ToString(),
                GlobalUsername = ctx.User.Username,
                Discriminator = ctx.User.Discriminator,
                Nickname = ctx.Member?.Nickname ?? ctx.User.Username,
                AvatarUrl = ctx.User.AvatarUrl ?? ctx.User.DefaultAvatarUrl,
            };
            var reminder = new Reminder
            {
                Member = member,
                Content = message,
                GuildId = ctx.Guild.Id.ToString(),
                ChannelId = ctx.Channel.Id.ToString(),
                CreatedAt = DateTime.UtcNow,
                CompleteAt = DateTime.UtcNow.AddMilliseconds((long)duration.Value),
                Duration = TimeSpan.FromMilliseconds((long)duration.Value),
            };
            
            var msg = new DiscordMessageBuilder()
                .WithContent($"Reminder set for {reminder.CompleteAt} with message: {message}");
            await ctx.RespondAsync(msg);
        }
    }
}
