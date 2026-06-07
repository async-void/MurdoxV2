using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using MurdoxV2.Extensions;
using MurdoxV2.Interfaces;
using MurdoxV2.Models;
using MurdoxV2.Pagination;
using MurdoxV2.Services.Builders;
using System.ComponentModel;

namespace MurdoxV2.SlashCommands.Member.Reminders
{
    [Command("reminder")]
    [Description("Manage member reminders.")]
    public class ReminderCommand(IReminderData reminderData, IReminder reminderService, IMemberData memberService, 
        IDiscordEmbedBuilder builderService, ILogger<ReminderCommand> logger)
    {
        #region REMIND
        [Command("remind")]
        [Description("Set a reminder")]
        public async Task SetReminderAsync(CommandContext ctx,
            [Description("Reminder Title")] string title,
            [Description("The time for the reminder in a format like '1h 30m' or '2d 3h'.")] string time,
            [Description("The message for the reminder.")] string message)
        {
            await ctx.DeferResponseAsync();
            var duration = reminderData.ParseTimeString(time);
            var timestamp = DateTimeOffset.UtcNow.AddMilliseconds(duration.Value);
            var unixTimestamp = timestamp.ToUnixTimeSeconds();
            var discordTimestamp = $"<t:{unixTimestamp}:R>";
            var msgTimestamp = DateTimeOffset.UtcNow.ToTimestamp(); ;
            var memberResult = await memberService.GetMemberAsync(ctx.Guild!.Id, ctx.User.Id);

            if (memberResult.IsOk)
            {

                var _reminder = new Reminder
                {
                    Title = title,
                    ServerMemberId = memberResult.Value.Id,
                    DiscordId = memberResult.Value.DiscordId,
                    Content = message,
                    GuildId = ctx.Guild.Id,
                    ChannelId = ctx.Channel.Id,
                    CreatedAt = DateTimeOffset.UtcNow.ToUniversalTime(),
                    CompleteAt = DateTimeOffset.UtcNow.ToUniversalTime().AddMilliseconds((long)duration.Value),
                    Duration = TimeSpan.FromMilliseconds((long)duration.Value),
                };
                var _reminderResult = await reminderService.SaveReminderAsync(_reminder);
                if (_reminderResult.IsOk)
                {
                    DiscordComponent[] components =
                    [
                        new DiscordTextDisplayComponent($"**Reminder** {message} set!"),
                        new DiscordSeparatorComponent(true),
                        new DiscordTextDisplayComponent($"I will remind you {discordTimestamp}"),
                        new DiscordSeparatorComponent(true, DiscordSeparatorSpacing.Large),
                        new DiscordSectionComponent(new DiscordTextDisplayComponent($"Murdox ©️ {msgTimestamp}"),
                            new DiscordButtonComponent(DiscordButtonStyle.Primary, "donateBtn", "Donate"))
                    ];

                    var container = new DiscordContainerComponent(components, false, DiscordColor.Teal);
                    var msg = new DiscordMessageBuilder()
                        .EnableV2Components()
                        .AddContainerComponent(container);
                    await ctx.RespondAsync(msg);
                    return;
                }
                else
                {
                    await ctx.RespondAsync($"Error setting reminder: {_reminderResult.Error.ErrorMessage}");
                    return;
                }
            }
            else
            {
                var member = new ServerMember()
                {
                    GuildId = ctx.Guild!.Id,
                    DiscordId = ctx.User.Id,
                    GlobalUsername = ctx.User.Username,
                    Discriminator = ctx.User.Discriminator,
                    Nickname = ctx.Member?.Nickname ?? ctx.User.Username,
                    AvatarUrl = ctx.User.AvatarUrl ?? ctx.User.DefaultAvatarUrl,
                    JoinedAt = DateTimeOffset.UtcNow.ToUniversalTime(),
                    CreatedAt = DateTimeOffset.UtcNow.ToUniversalTime(),
                    IsBot = ctx.User.IsBot,
                    IsMuted = false,
                    IsBanned = false,
                    MessageCount = 0,
                };
                var reminder = new Reminder
                {
                    Title = title,
                    Member = member,
                    DiscordId = ctx.User.Id,
                    Content = message,
                    GuildId = ctx.Guild.Id,
                    ChannelId = ctx.Channel.Id,
                    CreatedAt = DateTimeOffset.UtcNow.ToUniversalTime(),
                    CompleteAt = DateTimeOffset.UtcNow.AddMilliseconds((long)duration.Value),
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
        }
        #endregion

        #region LIST
        [Command("list")]
        [Description("List all reminders for the member.")]
        public async Task ListRemindersAsync(SlashCommandContext ctx)
        {
            var remindersResult = await reminderData.GetMemberRemindersAsync(ctx.Guild!.Id, ctx.User.Id);
            if (!remindersResult.IsOk)
            {
                await ctx.RespondAsync($"Error retrieving reminders: {remindersResult.Error.ErrorMessage}");
                logger.LogError("Error retrieving reminders for user {User} in guild {Guild}: {ErrorMessage}",
                    ctx.User.Username, ctx.Guild.Name, remindersResult.Error.ErrorMessage);
                return;
            }
            var reminders = remindersResult.Value;
            if (reminders.Count == 0)
            {
                await ctx.RespondAsync("You have no reminders set.");
                return;
            }

            var reminderChunks = reminderData.ChunkReminders(reminders, 4);
            await PaginationService.SendPaginatedMessageAsync(
                 ctx.Channel,
                 [.. reminderChunks.Select((chunk, i) =>
                     builderService.BuildReminderPage(chunk, i + 1, reminderChunks.Count)
                 ).OrderByDescending(x => x.Id)],
                 "reminders"
             );
        } 
        #endregion
    }
}
