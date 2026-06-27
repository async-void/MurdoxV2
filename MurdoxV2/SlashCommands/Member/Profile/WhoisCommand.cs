using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Humanizer;
using Microsoft.Extensions.Logging;
using MurdoxV2.Extensions;
using MurdoxV2.Interfaces;
using MurdoxV2.Services.Builders.Profile;
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
    public class WhoisCommand(IMemberData memberService, ILogger<WhoisCommand> logger, ProfileImageBuilderService profileBuilder)
    {
        private readonly ProfileImageBuilderService _profileBuilder = profileBuilder;

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
                var memRoles = guildMember.Roles.ToList();
                var memberDTO = new MemberProfileDTO
                {
                    GuildId = guildMember.Guild.Id,
                    MemberId = guildMember.Id,
                    CreatedAt = guildMember.CreationTimestamp,
                    NickName = guildMember.Nickname,
                    GuildTag = guildMember.PrimaryGuild?.Tag ?? "UNKOWN",
                    MemberAvatarUrl = guildMember.AvatarUrl,
                    GlobalUsername = user.Username,
                    Roles = memRoles,
                };

                if (dbMember.IsOk)
                {
                    var joinedAt = guildMember.JoinedAt - DateTimeOffset.UtcNow;
                    var userStatus = guildMember.Presence?.Status.ToString() ?? "Unknown";
                    var balance = dbMember.Value.Bank?.Balance ?? 0;
                    var reminderCount = dbMember.Value.Reminders?.Count ?? 0;
                    var ticketCount = dbMember.Value.Tickets?.Count ?? 0;
                    var memDescription = $"Nickname: {dbMember.Value.Nickname}\t\tBalance: {balance}\rJoined: {joinedAt.Humanize()}\rStatus: {userStatus}\rReminders: {reminderCount}\rTickets: {ticketCount}";
                    memberDTO.Bank = dbMember.Value.Bank;
                    memberDTO.Tickets = dbMember.Value.Tickets?.ToList() ?? [];
                    memberDTO.Reminders = dbMember.Value.Reminders?.ToList() ?? [];
                    memberDTO.XP = dbMember.Value.XP;
                    memberDTO.JoinedGuildAt = guildMember.JoinedAt;

                    var profileImage = await _profileBuilder.BuildProfileImageAsync(memberDTO);

                    var builder = new DiscordWebhookBuilder() // or DiscordInteractionResponseBuilder
                           .EnableV2Components()
                           .AddFile("profile.png", profileImage);

                    var mediaItem = new DiscordMediaGalleryItem("attachment://profile.png");
                    var mediaGallery = new DiscordMediaGalleryComponent(mediaItem);

                    var container = new DiscordContainerComponent(
                        [
                            new DiscordTextDisplayComponent($"Profile for {guildMember.Mention}!"),
                            mediaGallery
                        ],
                        color: new DiscordColor("#7160e8")
                    );

                    builder.AddContainerComponent(container);
                    await ctx.RespondAsync(builder);
                }
                else
                {
                    var verifiedStatus = guildMember?.Verified ?? true ? "Yes" : "No";
                    var userStatus = guildMember?.Presence?.Status.ToString() ?? "Unknown";
                    var nickName = guildMember?.Nickname ?? guildMember?.DisplayName ?? "Unknown";
                    var joinedAt = guildMember.JoinedAt - DateTimeOffset.UtcNow;
                    var memDescription = $"Nickname: {nickName}\t\tJoined: {joinedAt.Humanize()}\rVerified: {verifiedStatus}\rStatus: {userStatus}";
                    memberDTO.UserStatus = verifiedStatus;
                    memberDTO.VerifiedStatus = verifiedStatus;
                    var profileImage = await _profileBuilder.BuildProfileImageAsync(memberDTO);

                    var builder = new DiscordWebhookBuilder() // or DiscordInteractionResponseBuilder
                           .EnableV2Components()
                           .AddFile("profile.png", profileImage);

                    var mediaItem = new DiscordMediaGalleryItem("attachment://profile.png");
                    var mediaGallery = new DiscordMediaGalleryComponent(mediaItem);

                    var container = new DiscordContainerComponent(
                        [
                            new DiscordTextDisplayComponent($"Profile for {guildMember.Mention}!"),
                            mediaGallery
                        ],
                        color: new DiscordColor("#7160e8")
                    );

                    builder.AddContainerComponent(container);
                    await ctx.RespondAsync(builder);
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

