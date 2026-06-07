using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Humanizer;
using Microsoft.Extensions.Logging;
using MurdoxV2.Extensions;
using MurdoxV2.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.SlashCommands.Member.Profile
{
    public class WhoisCommand(IMemberData memberService, ILogger<WhoisCommand> logger)
    {
        #region WHOIS
        [Command("whois")]
        [Description("Displays information about a user.")]
        public async Task Whois(CommandContext ctx, [Parameter("member")] DiscordUser user)
        {
            try
            {
                await ctx.DeferResponseAsync();
                var timestamp = DateTimeOffset.UtcNow.ToTimestamp();
                // Fetch the member from the guild
                var guildMember = user is DiscordMember member ? member
                    : await ctx.Guild!.GetMemberAsync(user.Id);
                // Fetch the member from the database
                var dbMember = await memberService.GetMemberAsync(ctx.Guild!.Id, user.Id);

                if (dbMember.IsOk)
                {
                    var joinedAt = guildMember.JoinedAt - DateTimeOffset.UtcNow;
                    var userStatus = guildMember.Presence?.Status.ToString() ?? "Unknown";
                    var balance = dbMember.Value.Bank?.Balance ?? 0;
                    var reminderCount = dbMember.Value.Reminders?.Count ?? 0;
                    var ticketCount = dbMember.Value.Tickets?.Count ?? 0;
                    var memDescription = $"Nickname: {dbMember.Value.Nickname}\t\tBalance: {balance}\rJoined: {joinedAt.Humanize()}\rStatus: {userStatus}\rReminders: {reminderCount}\rTickets: {ticketCount}";

                    DiscordComponent[] comps =
                    [
                        new DiscordTextDisplayComponent($"## Whois"),
                        new DiscordSectionComponent(new DiscordTextDisplayComponent($"## {dbMember.Value.Nickname}"),
                        new DiscordThumbnailComponent(dbMember.Value.AvatarUrl)),
                        new DiscordSeparatorComponent(true),
                        new DiscordTextDisplayComponent(memDescription),
                        new DiscordSeparatorComponent(true, DiscordSeparatorSpacing.Large),
                        new DiscordSectionComponent(new DiscordTextDisplayComponent($"MURDoX ©️ {timestamp}"),
                        new DiscordButtonComponent(DiscordButtonStyle.Primary, "donateId", "Donate"))
                    ];
                    var container = new DiscordContainerComponent(comps, false, DiscordColor.Grayple);
                    var msg = new DiscordMessageBuilder()
                        .EnableV2Components()
                        .AddContainerComponent(container);
                    await ctx.RespondAsync(msg);
                }
                else
                {
                    var verifiedStatus = (bool?)guildMember?.Verified ?? true ? "Yes" : "No";
                    var userStatus = guildMember?.Presence?.Status.ToString() ?? "Unknown";
                    var nickName = guildMember?.Nickname ?? guildMember?.DisplayName ?? "Unknown";
                    var joinedAt = guildMember.JoinedAt - DateTimeOffset.UtcNow;
                    var memDescription = $"Nickname: {nickName}\t\tJoined: {joinedAt.Humanize()}\rVerified: {verifiedStatus}\rStatus: {userStatus}";
                    DiscordComponent[] comps =
                    [
                        new DiscordTextDisplayComponent($"## Whois"),
                        new DiscordSectionComponent(new DiscordTextDisplayComponent($"## {nickName}"),
                        new DiscordThumbnailComponent(guildMember.AvatarUrl)),
                        new DiscordSeparatorComponent(true),
                        new DiscordTextDisplayComponent(memDescription),
                        new DiscordSeparatorComponent(true, DiscordSeparatorSpacing.Large),
                        new DiscordSectionComponent(new DiscordTextDisplayComponent($"MURDoX ©️ {timestamp}"),
                        new DiscordButtonComponent(DiscordButtonStyle.Primary, "donateId", "Donate"))
                    ];
                    var container = new DiscordContainerComponent(comps, false, DiscordColor.Grayple);
                    var msg = new DiscordMessageBuilder()
                        .EnableV2Components()
                        .AddContainerComponent(container);
                    await ctx.RespondAsync(msg);
                }
            }
            catch (Exception e)
            {
                logger.LogError("something went wrong! {ctx}, the api might have thrown a 'not found' error\r\n{exception}", Assembly.GetCallingAssembly().FullName, e.Message);
            }
        }
        #endregion
    }
}

