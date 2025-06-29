using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Humanizer;
using Humanizer.Localisation;
using MurdoxV2.Interfaces;
using MurdoxV2.Models;
using System.ComponentModel;

namespace MurdoxV2.SlashCommands.Member.Reminders
{
    [Command("reminder")]
    [Description("Manage member reminders.")]
    public class ReminderCommand(IReminderData reminderData, IReminder reminderService)
    {
        #region REMIND
        [Command("remind")]
        [Description("Set a reminder")]
        public async Task SetReminderAsync(CommandContext ctx,
            [Description("The time for the reminder in a format like '1h 30m' or '2d 3h'.")] string time,
            [Description("The message for the reminder.")] string message)
        {
            await ctx.DeferResponseAsync();
            var duration = reminderData.ParseTimeString(time);
            var timestamp = DateTimeOffset.UtcNow.AddMilliseconds(duration.Value);
            var unixTimestamp = timestamp.ToUnixTimeSeconds();
            var discordTimestamp = $"<t:{unixTimestamp}:R>";

            var bank = new Bank()
            {
                Balance = 100,
                Deposit_Amount = 100,
                Deposit_Timestamp = DateTime.UtcNow,    
            };

            var member = new ServerMember()
            {
                GuildId = ctx.Guild!.Id.ToString(),
                DiscordId = ctx.User.Id.ToString(),
                GlobalUsername = ctx.User.Username,
                Discriminator = ctx.User.Discriminator,
                Nickname = ctx.Member?.Nickname ?? ctx.User.Username,
                AvatarUrl = ctx.User.AvatarUrl ?? ctx.User.DefaultAvatarUrl,
                Bank = bank,
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
            var reminderResult = await reminderService.SaveReminderAsync(reminder);

            if (reminderResult.IsOk)
            {
                DiscordComponent[] components =
                {
                    new DiscordTextDisplayComponent($"**Reminder** {message} set!"),
                    new DiscordSeparatorComponent(true),
                    new DiscordTextDisplayComponent($"I will remind you {discordTimestamp}"),
                    new DiscordSeparatorComponent(true, DiscordSeparatorSpacing.Large),
                    new DiscordSectionComponent(new DiscordTextDisplayComponent($"Murdox ©️ {DateTime.UtcNow.Year}"),
                        new DiscordButtonComponent(DiscordButtonStyle.Primary, "donateBtn", "Donate"))
                };

                var container = new DiscordContainerComponent(components, false, DiscordColor.Teal);
                var msg = new DiscordMessageBuilder()
                    .EnableV2Components()
                    .AddContainerComponent(container);
                await ctx.RespondAsync(msg);
            }
            else
                await ctx.RespondAsync($"Error setting reminder: {reminderResult.Error.ErrorMessage}");


        }
        #endregion

        #region LIST
        [Command("list")]
        [Description("List all reminders for the member.")]
        public async Task ListRemindersAsync(CommandContext ctx)
        {
            await ctx.DeferResponseAsync();
            var remindersResult = await reminderData.GetMemberRemindersAsync(ctx.Guild!.Id.ToString(), ctx.User.Id.ToString());
            if (!remindersResult.IsOk)
            {
                await ctx.RespondAsync($"Error retrieving reminders: {remindersResult.Error.ErrorMessage}");
                return;
            }
            var reminders = remindersResult.Value;
            if (reminders.Count == 0)
            {
                await ctx.RespondAsync("You have no reminders set.");
                return;
            }
            var response = new DiscordMessageBuilder()
                .WithContent("Your reminders:\n" + string.Join("\n", reminders.Select(r => $"{r.Id}: {r.Content} (Due: {r.CompleteAt})")));
            await ctx.RespondAsync(response);
        } 
        #endregion
    }
}
