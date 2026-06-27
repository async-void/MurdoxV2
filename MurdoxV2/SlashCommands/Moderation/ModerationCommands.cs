using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MurdoxV2.Data.DbContext;
using MurdoxV2.Extensions;
using MurdoxV2.Helpers;
using MurdoxV2.Interfaces;
using MurdoxV2.Models;
using MurdoxV2.RoleCheck;
using MurdoxV2.Utilities.Timestamp;
using System.ComponentModel;

namespace MurdoxV2.SlashCommands.Moderation
{
    [Command("moderation")]
    [Description("Moderation commands for managing the server.")]
    [RequirePermissions(DiscordPermission.ManageGuild)]
    public class ModerationCommands(IDbContextFactory<AppDbContext> dbFactory, IMemberData memberService, ILogger<ModerationCommands> logger,
        RateLimitHelper<ulong> rateHelper)
    {
        private readonly IMemberData _memberService = memberService;
        private readonly RateLimitHelper<ulong> _rateHelper = rateHelper;
        

        #region PURGE
        [Command("purge")]
        [Description("remove a set number of messages from the channel with rate limit for chunks of 100 messages")]
        public async ValueTask Purge(SlashCommandContext ctx, [Parameter("amount")] int amount)
        {
            await ctx.DeferResponseAsync();
            var originalResponse = await ctx.GetResponseAsync();

            var messages = new List<DiscordMessage>();
            await foreach (var message in ctx.Channel.GetMessagesAsync(amount)) 
            {
                if (message.Id == originalResponse.Id)
                    continue;
                messages.Add(message);
            }

            var cutoff = DateTimeOffset.UtcNow.AddDays(-14);
            var recent = messages.Where(m => m.CreationTimestamp > cutoff).ToList();
            var old = messages.Where(m => m.CreationTimestamp <= cutoff).ToList();
            var totalMsgs = messages.Count;
            var deleted = 0;

            var progressMsg = await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent($"Deleting messages... 0/{totalMsgs}"));

            foreach (var chunk in recent.Chunk(100))
            {
                //await _rateHelper.WaitForRateLimitAsync(ctx.Channel.Id);
                await ctx.Channel.DeleteMessagesAsync(chunk);

                deleted+= chunk.Length;
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent($"Deleting messages... {deleted}/{totalMsgs}"));
            }
            foreach(var message in old)
            {
                await _rateHelper.WaitForRateLimitAsync(ctx.Channel.Id);
                await ctx.Channel.DeleteMessageAsync(message);

                deleted++;
                if (deleted % 5 == 0 || deleted == totalMsgs)
                {
                    var remaining = totalMsgs - deleted;
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent($"Deleting messages... {deleted}/{totalMsgs}  [reamining {remaining}]"));
                }
            }

            var amountSuffix = totalMsgs == 1 ? "message" : "messages";
            var components = new DiscordComponent[]
            {
                new DiscordTextDisplayComponent("**Purge**"),
                new DiscordSeparatorComponent(true),
                new DiscordTextDisplayComponent($":right_arrow: Done , I removed ``{totalMsgs}`` {amountSuffix} from ``{ctx.Channel.Name}``"),
                new DiscordSeparatorComponent(true, DiscordSeparatorSpacing.Large),
                new DiscordSectionComponent(new DiscordTextDisplayComponent($"-# Murdox ©️ {DateTime.UtcNow:ddd, MM-dd-yyyy hh:mm tt}"),
                                        new DiscordButtonComponent(DiscordButtonStyle.Secondary, "donateBtn", "Donate")),
            };

            var container = new DiscordContainerComponent(components, false, DiscordColor.Violet);

            var msg = new DiscordWebhookBuilder()
                .EnableV2Components()
                .AddContainerComponent(container);
            await ctx.EditResponseAsync(msg);

        }
        #endregion

        #region ADD_XP
        [Command("add-xp")]
        [Description("add XP to a user in the guild.")]
        public async Task AddXp(CommandContext ctx, [Parameter("user")] DiscordUser user, [Parameter("amount")] int amount)
        {
            await ctx.DeferResponseAsync();
            var member = await _memberService.GetMemberAsync(ctx.Guild!.Id, user.Id);

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
                    DiscordId = user.Id,
                    GuildId = ctx.Guild.Id,
                    XP = amount,
                    GlobalUsername = user?.GlobalName ?? "UNKNOWN",
                    Nickname = user?.Username ?? "UNKNOWN",
                    AvatarUrl = user?.AvatarUrl ?? string.Empty,
                    Discriminator = user?.Discriminator ?? string.Empty,
                    Bank = bank,
                };

                var success = await _memberService.SaveMemberAsync(mem);
                if (success.IsOk)
                    await ctx.RespondAsync($":right_arrow: User {mem?.GlobalUsername ?? "UNKNOWN"} now has ``{mem?.Bank.Balance ?? 0}`` XP");
                else
                    await ctx.RespondAsync($":right_arrow: there was an error saving member: {mem.GlobalUsername} to the database\r\r{success.Error.ErrorMessage}");
                return;
            }
            member.Value.XP += amount;
            //TODO: Update member data.
            await ctx.RespondAsync($"Added {amount} XP to {user.Username}. New XP: {member.Value.XP}");
        }
        #endregion

        #region LOCK CHANNEL
        [Command("lock-channel")]
        [Description("deny all send permissions for channel")]
        public async ValueTask LockChannel(SlashCommandContext ctx, [Parameter("reason")] string reason)
        {
            await ctx.DeferResponseAsync();
            var timestamp = DateTimeOffset.UtcNow.ToTimestamp();
            var author = ctx.Member?.Username ?? "Unknown";
            var channel = ctx.Channel;
            var everyOneRole = ctx.Guild!.EveryoneRole;
            await channel.AddOverwriteAsync(everyOneRole, deny: DiscordPermissions.All);

            DiscordComponent[] comps =
            [
                new DiscordTextDisplayComponent("## Channel Locked"),
                new DiscordSeparatorComponent(true),
                new DiscordTextDisplayComponent($"{author} locked channel"),
                new DiscordTextDisplayComponent($"Reason: {reason}"),
                new DiscordSeparatorComponent(true),
                new DiscordSectionComponent(new DiscordTextDisplayComponent($"Murdox ©️ {timestamp}"), 
                    new DiscordButtonComponent(DiscordButtonStyle.Secondary, "donateBtn", "Donate"))
            ];
            var container = new DiscordContainerComponent(comps, false, DiscordColor.LightGray);
            var msg = new DiscordMessageBuilder()
                .EnableV2Components()
                .AddContainerComponent(container);

            logger.LogInformation("{mod} locked channel: {channel} in Guild: {guild}", author, channel.Name, ctx.Guild!.Name);
            await ctx.RespondAsync(msg);
        }
        #endregion

        #region UNLOCK CHANNEL
        [Command("unlock-channel")]
        [Description("allow all send permissions for channel")]
        public async ValueTask UnLockChannel(SlashCommandContext ctx)
        {
            await ctx.DeferResponseAsync();
            var timestamp = DateTimeOffset.UtcNow.ToTimestamp();
            var author = ctx.Member?.Username ?? "Unknown";
            var channel = ctx.Channel;
            var everyOneRole = ctx.Guild!.EveryoneRole;
            await channel.AddOverwriteAsync(everyOneRole, allow: DiscordPermissions.All);

            DiscordComponent[] comps =
           [
               new DiscordTextDisplayComponent("## Channel UnLocked"),
                new DiscordSeparatorComponent(true),
                new DiscordTextDisplayComponent($"{author} unlocked channel"),
                new DiscordSeparatorComponent(true),
                new DiscordSectionComponent(new DiscordTextDisplayComponent($"Murdox ©️ {timestamp}"),
                    new DiscordButtonComponent(DiscordButtonStyle.Secondary, "donateBtn", "Donate"))
           ];
            var container = new DiscordContainerComponent(comps, false, DiscordColor.LightGray);
            var msg = new DiscordMessageBuilder()
                .EnableV2Components()
                .AddContainerComponent(container);

            logger.LogInformation("{mod} unlocked channel: {channel} in Guild: {guild}", author, channel.Name, ctx.Guild.Name);
            await ctx.RespondAsync(msg);
        }
        #endregion

        #region ENABLE SLOWMODE
        [Command("enable-slowmode")]
        [Description("enable slowmode for channel")]
        public async ValueTask EnableSlowmode(SlashCommandContext ctx, [Parameter("interval")] int interval)
        {
            await ctx.DeferResponseAsync();
            var timestamp = DateTimeOffset.UtcNow.ToTimestamp();
            var author = ctx.Member?.Username ?? "Unknown";
            var channel = ctx.Channel;

            await channel.ModifyAsync(c => c.PerUserRateLimit = interval);
            DiscordComponent[] comps =
           [
               new DiscordTextDisplayComponent("## Slowmode Enabled"),
                new DiscordSeparatorComponent(true),
                new DiscordTextDisplayComponent($"{author} enabled slowmode with {interval} second interval"),
                new DiscordSeparatorComponent(true),
                new DiscordSectionComponent(new DiscordTextDisplayComponent($"Murdox ©️ {timestamp}"),
                    new DiscordButtonComponent(DiscordButtonStyle.Secondary, "donateBtn", "Donate"))
           ];
            var container = new DiscordContainerComponent(comps, false, DiscordColor.LightGray);
            var msg = new DiscordMessageBuilder()
                .EnableV2Components()
                .AddContainerComponent(container);
            await ctx.RespondAsync(msg);
        }
        #endregion

        #region DISABLE SLOWMODE
        [Command("disable-slowmode")]
        [Description("disable slowmode for channel")]
        public async ValueTask DisableSlowmode(SlashCommandContext ctx)
        {
            await ctx.DeferResponseAsync();
            var timestamp = DateTimeOffset.UtcNow.ToTimestamp();
            var author = ctx.Member?.Username ?? "Unknown";
            var channel = ctx.Channel;

            await channel.ModifyAsync(c => c.PerUserRateLimit = 0);
            DiscordComponent[] comps =
           [
               new DiscordTextDisplayComponent("## Slowmode Enabled"),
                new DiscordSeparatorComponent(true),
                new DiscordTextDisplayComponent($"{author} disabled slowmode"),
                new DiscordSeparatorComponent(true),
                new DiscordSectionComponent(new DiscordTextDisplayComponent($"Murdox ©️ {timestamp}"),
                    new DiscordButtonComponent(DiscordButtonStyle.Secondary, "donateBtn", "Donate"))
           ];
            var container = new DiscordContainerComponent(comps, false, DiscordColor.LightGray);
            var msg = new DiscordMessageBuilder()
                .EnableV2Components()
                .AddContainerComponent(container);
            await ctx.RespondAsync(msg);
        }
        #endregion

        #region SET TIMEOUT FOR MEMBER
        [Command("timeout")]
        [Description("timeout a member for a specified amount of time")]
        public async ValueTask SetTimeout(SlashCommandContext ctx, [Parameter("member")] DiscordUser user, [Parameter("duration")] string duration, [Parameter("reason")] string reason)
        {
            await ctx.DeferResponseAsync(true);
            var timestamp = DateTimeOffset.UtcNow.ToTimestamp();
            if (ctx.Guild is null)
            {
                await ctx.RespondAsync("This command can only be used inside a server.");
                return;
            }

            var (Success, Value, Unit) = TimestampDataProvider.ValidateTimeout(duration);
            if (!Success)
            {
                await ctx.RespondAsync("input in wrong format"); 
                return;
            }

            var member = await ctx.Guild.GetMemberAsync(user.Id);
            var timeoutUntil = TimestampDataProvider.ParseTimeout(Value, Unit);
            var timedOutDuration = timeoutUntil.Humanize();
            await member.TimeoutAsync(timeoutUntil);
            await Task.Delay(500);

            DiscordComponent[] comps =
            [
                new DiscordTextDisplayComponent("## Timed Out"),
                new DiscordSeparatorComponent(true),
                new DiscordTextDisplayComponent($"Guild: {ctx.Guild.Name}"),
                new DiscordTextDisplayComponent($"Channel: {ctx.Channel.Name}"),
                new DiscordTextDisplayComponent($"Duration: {timedOutDuration}"),
                new DiscordTextDisplayComponent($"Reason: {reason}"),
                new DiscordSeparatorComponent(true),
                new DiscordTextDisplayComponent($"Murdox ©️ {timestamp}")
                
            ];
            var container = new DiscordContainerComponent(comps, false, DiscordColor.DarkRed);
            var msg = new DiscordMessageBuilder()
                .EnableV2Components()
                .AddContainerComponent(container);
            await member.SendMessageAsync(msg);
            await ctx.RespondAsync($"{member.DisplayName} has been timed out until {timedOutDuration}");
        }
        #endregion

        #region REMOVE TIMEOUT FOR MEMBER
        [Command("remove-timeout")]
        [Description("remove timeout for member")]
        public async ValueTask RemoveSlowmode(SlashCommandContext ctx, [Parameter("member")] DiscordUser user)
        {
            await ctx.DeferResponseAsync();
            var member = await ctx.Guild!.GetMemberAsync(user.Id, false);
            await member.TimeoutAsync(null);
        }
        #endregion

        #region REMOVE MEMBER MESSAGE
        [Command("remove-message")]
        [Description("remove a member's message by the message id")]
        public async ValueTask RemoveMsgById(SlashCommandContext ctx, [Parameter("id")] [Description("the message id")] ulong id,
            [Parameter("reason")][Description("why the mseeage is being removed")] string reason) 
        {
            await ctx.DeferResponseAsync(ephemeral: true);
            var msgToRemove = await ctx.Channel.GetMessageAsync(id);
            await msgToRemove.DeleteAsync();

            await Task.Delay(200);
            await msgToRemove.Author!.SendMessageAsync($"your message in Guild {msgToRemove.Channel!.Guild.Name} Channel: {msgToRemove.Channel.Name} was removed | Reason: {reason}");
        }
        #endregion

        #region REMOVE ALL MEMBER MESSAGES
        [Command("remove-all-user-messages")]
        [Description("remove all messages from a member in the channel")]
        public async ValueTask RemoveAllUserMessages(SlashCommandContext ctx, [Parameter("member")] DiscordUser user, [Parameter("reason")] string? reason = null)
        {
            await ctx.DeferResponseAsync();
            var member = await ctx.Guild!.GetMemberAsync(user.Id, false);

            await foreach (var msg in ctx.Channel.GetMessagesAsync())
            {
                if (msg.Author!.Id == member.Id)
                {
                    await msg.DeleteAsync();
                    await Task.Delay(200);
                } 
            }
            var finalReason = reason ?? "No reason provided";
            await user.SendMessageAsync($"all your messages in Guild {ctx.Guild!.Name} Channel: {ctx.Channel.Name} were removed | Reason: {finalReason}");
        }
        #endregion

        #region REMOVE INACTIVE MEMBERS

        [Command("configure")]
        [Description("configure server settings")]
        public async ValueTask Configure(SlashCommandContext ctx)
        {
            await ctx.DeferResponseAsync();
            
        }

        #endregion

        #region CONFIGURE HONEYPOT CHANNEL      

        [Command("configure-honeypot")]
        [Description("add a honeypot channel to the server")]
        public async ValueTask ConfigureHoneypot(SlashCommandContext ctx)
        {
            await ctx.DeferResponseAsync();
            using var db = dbFactory.CreateDbContext();
            var guildObj = db.Guilds.FirstOrDefault(x => x.GuildId == ctx.Guild!.Id);

            if (guildObj != null)
            {
                if (guildObj.HoneypotChannelId == 0)
                {
                    var chnl = await ctx.Guild!.CreateChannelAsync("honeypot", DiscordChannelType.Text, null, "do not post messages here, you will be kicked then banned from this server");
                    guildObj.HoneypotChannelId = chnl.Id;
                    db.Guilds.Update(guildObj);
                    await db.SaveChangesAsync();
                }
            }    
        }

        #endregion

        #region BAN MEMBER WITH REASON
        [Command("ban")]
        [Description("ban a member from the server with a reason")]
        public async ValueTask BanMember(SlashCommandContext ctx, [Parameter("member")] DiscordUser user, [Parameter("reason")] string reason)
        {
            await ctx.DeferResponseAsync();
            var timestamp = DateTimeOffset.UtcNow.ToTimestamp();
            var member = await ctx.Guild!.GetMemberAsync(user.Id);
            await member.BanAsync(TimeSpan.FromDays(1), reason);
            
            DiscordComponent[] comps =
            [
                new DiscordTextDisplayComponent("## Member Banned"),
                new DiscordSeparatorComponent(true),
                new DiscordTextDisplayComponent($"Guild: {ctx.Guild.Name}"),
                new DiscordTextDisplayComponent($"Channel: {ctx.Channel.Name}"),
                new DiscordTextDisplayComponent($"Reason: {reason}"),
                new DiscordSeparatorComponent(true),
                new DiscordTextDisplayComponent("to dispute this ban, please contact the server staff or appeal on the support server"),
                new DiscordSeparatorComponent(true),
                new DiscordTextDisplayComponent($"Murdox ©️ {timestamp}")
                
            ];
            var container = new DiscordContainerComponent(comps, false, DiscordColor.DarkRed);
            var msg = new DiscordMessageBuilder()
                .EnableV2Components()
                .AddContainerComponent(container);
            await member.SendMessageAsync(msg);
            await ctx.RespondAsync($"{member.DisplayName} has been banned from the server.");
        }
        #endregion

        #region UNBAN MEMBER
        [Command("unban")]
        [Description("unban a member from the server")]
        public async ValueTask UnBan(SlashCommandContext ctx, [Parameter("user")] DiscordUser user)
        {
            await ctx.DeferResponseAsync();
            var guild = ctx.Guild!;
            await guild.UnbanMemberAsync(user.Id);

            var author = ctx.Interaction.Message?.Author?.Username ?? "Unknown";
            DiscordComponent[] comps =
            [
                new DiscordTextDisplayComponent("## Member Unbanned"),
                new DiscordSeparatorComponent(true),
                new DiscordTextDisplayComponent($"a mod has removed the server ban for {user.Username}"),
                new DiscordTextDisplayComponent($"Guild: {ctx.Guild?.Name ?? "UnKnown"}"),
                new DiscordTextDisplayComponent($"Channel: {ctx.Channel?.Name ?? "UnKnown"}"),
                new DiscordSeparatorComponent(true),
                new DiscordTextDisplayComponent($"Murdox ©️ {DateTimeOffset.UtcNow.ToTimestamp()}")
                
            ];
            var container = new DiscordContainerComponent(comps, false, DiscordColor.DarkGreen);
            var msg = new DiscordMessageBuilder()
                .EnableV2Components()
                .AddContainerComponent(container);
            await ctx.RespondAsync($"{author} has removed a server ban for {user.Mention}");
            await Task.Delay(500);
            await user.SendMessageAsync(msg);
        }
        #endregion

        #region TOGGLE ALLOW URLS

        [Command("allow-urls")]
        [Description("toggle allow urls in the server")]
        public async ValueTask ToggleAllowUrls(SlashCommandContext ctx, [Parameter("toggle")] bool toggle)
        {
            await ctx.DeferResponseAsync();
            var timestamp = DateTimeOffset.UtcNow.ToTimestamp();
            using var db = await dbFactory.CreateDbContextAsync();
            var allow = db.Guilds.Where(g => g.GuildId == ctx.Guild!.Id).Select(g => g.AllowUrls).FirstOrDefault();
            var status = toggle ? "enabled" : "disabled";
            DiscordComponent[] comps =
           [
               new DiscordTextDisplayComponent("## URL Settings Updated"),
                new DiscordSeparatorComponent(true),
                new DiscordTextDisplayComponent($"a mod has {status} urls in the server"),
                new DiscordTextDisplayComponent($"Guild: {ctx.Guild?.Name ?? "unknown"}"),
                new DiscordTextDisplayComponent($"Channel: {ctx.Channel.Name}"),
                new DiscordSeparatorComponent(true),
                new DiscordTextDisplayComponent($"Murdox ©️ {timestamp}")
           ];
            var container = new DiscordContainerComponent(comps, false, DiscordColor.LightGray);
            var msg = new DiscordMessageBuilder()
                .EnableV2Components()
                .AddContainerComponent(container);
            await ctx.RespondAsync(msg);
        }

        #endregion
    }
}
