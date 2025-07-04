using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using MurdoxV2.Data.DbContext;
using MurdoxV2.Interfaces;
using MurdoxV2.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MurdoxV2.SlashCommands.Moderation
{
    [Command("moderation")]
    [Description("Moderation commands for managing the server.")]
    [RequirePermissions(DiscordPermission.ManageGuild)]
    public class ModerationCommands(IMemberData memberService)
    {
        private readonly IMemberData _memberService = memberService;

        #region PURGE
        [Command("purge")]
        [Description("remove a set number of messages from the channel (100 max)")]
        public async Task Purge(CommandContext ctx, [Parameter("amount")] int amount)
        {
            var rnd = new Random();
            await foreach (var message in ctx.Channel.GetMessagesAsync(amount))
            {
                var delay = rnd.Next(100, 300);
                await ctx.Channel.DeleteMessageAsync(message);
                await Task.Delay(delay);
            }

            var components = new DiscordComponent[]
            {
                new DiscordTextDisplayComponent("**Purge**"),
                new DiscordSeparatorComponent(true),
                new DiscordTextDisplayComponent($"Done , I removed ``{amount}`` messages from ``{ctx.Channel.Name}``"),
                new DiscordSeparatorComponent(true, DiscordSeparatorSpacing.Large),
                new DiscordSectionComponent(new DiscordTextDisplayComponent($"-# Murdox ©️ {DateTime.UtcNow:ddd, MM-dd-yyyy hh:mm tt}"),
                                        new DiscordButtonComponent(DiscordButtonStyle.Secondary, "donateBtn", "Donate")),
            };

            var container = new DiscordContainerComponent(components, false, DiscordColor.Violet);

            var msg = new DiscordMessageBuilder()
                .EnableV2Components()
                .AddContainerComponent(container);
            await ctx.Channel.SendMessageAsync(msg);

        }
        #endregion

        #region ADD_XP
        [Command("add-xp")]
        [Description("add XP to a user in the guild.")]
        public async Task AddXp(CommandContext ctx, [Parameter("user")] DiscordUser user, [Parameter("amount")] int amount)
        {
            await ctx.DeferResponseAsync();
            var member = await _memberService.GetMemberAsync(ctx.Guild!.Id.ToString(), user.Id.ToString());

            if (!member.IsOk)
            {
                var bank = new Bank
                {
                    Balance = 5000,
                    Deposit_Amount = 0,
                    Withdraw_Amount = 0,
                    Deposit_Timestamp = DateTime.UtcNow,
                };

                var mem = new ServerMember
                {
                    DiscordId = user.Id.ToString(),
                    GuildId = ctx.Guild.Id.ToString(),
                    XP = amount,
                    GlobalUsername = user.GlobalName,
                    Nickname = user.Username,
                    AvatarUrl = user.AvatarUrl ?? string.Empty,
                    Discriminator = user.Discriminator,
                    Bank = bank,
                };

                await ctx.RespondAsync($"User {user.Username} now has ``{amount}`` XP");
                return;
            }
            member.Value.XP += amount;

            await ctx.RespondAsync($"Added {amount} XP to {user.Username}. New XP: {member.Value.XP}");
        }
        #endregion
    }
}
