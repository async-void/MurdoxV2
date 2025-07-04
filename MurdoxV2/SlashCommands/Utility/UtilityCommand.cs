using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MurdoxV2.Data.DbContext;
using MurdoxV2.Factories;
using MurdoxV2.Interfaces;
using MurdoxV2.Models;
using MurdoxV2.Schedules;
using MurdoxV2.Services;
using MurdoxV2.Utilities.Timestamp;
using Quartz;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.SlashCommands.Utility
{
    [Command("utility")]
    [Description("Utility commands for various functionalities")]
    public class UtilityCommands(IDbContextFactory<AppDbContext> dbFactory, IFact factService)
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
        private readonly IFact _factService = factService;

        #region UPTIME
        [Command("uptime")]
        [Description("the time Murdox has been online since the last restart")]
        public async Task UptimeAsync(CommandContext ctx)
        {
            await ctx.DeferResponseAsync();

            var uptime = await TimestampDataProvider.GetBotUptimeAsync();

            DiscordComponent[] components =
              [
                  new DiscordTextDisplayComponent($"# Uptime  \t<:clock:1385915131799801988>"),
                                  new DiscordSeparatorComponent(true, DiscordSeparatorSpacing.Large),
                                  new DiscordTextDisplayComponent($"Murdox has been online for: {uptime}"),
                                  new DiscordSeparatorComponent(true),
                                  new DiscordSectionComponent(new DiscordTextDisplayComponent($"-# Murdox ©️ {DateTime.UtcNow:ddd, MM-dd-yyyy hh:mm tt}"),
                                        new DiscordButtonComponent(DiscordButtonStyle.Secondary, "donateBtn", "Donate")),
                              ];
            var container = new DiscordContainerComponent(components, false, DiscordColor.Azure);

            var msg = new DiscordMessageBuilder()
                .EnableV2Components()
                .AddContainerComponent(container);

            await ctx.EditResponseAsync(msg);
        }
        #endregion

        #region PING
        [Command("ping")]
        [Description("get latency info for the system")]
        public async Task PingAsync(CommandContext ctx)
        {
            await ctx.DeferResponseAsync();
            var sw = new Stopwatch();
            var guildId = ctx.Guild?.Id ?? 0;
            var discordPing = ctx.Client.GetConnectionLatency(guildId);

            sw.Start();
            using var db = _dbFactory.CreateDbContext();
            var dbLatency = await db.Reminders.FirstOrDefaultAsync();
            sw.Stop();
            var dbPing = sw.ElapsedMilliseconds;

            sw.Restart();
            var gatewayInfo = await ctx.Client.GetGatewayInfoAsync();
            var gatewayLatency = sw.ElapsedMilliseconds;
            sw.Stop();

            var dbLatencyString = dbPing > 1000 ? $"{dbPing / 1000.0:F2} seconds" : $"{dbPing} ms";
            var discordLatency = gatewayLatency > 1000 ? $"{discordPing / 1000.0:F2} seconds" : $"{gatewayLatency} ms";

            var guildCount = ctx.Client.Guilds.Count;

            var sb = new StringBuilder();
            sb.Append($"### Latency:\r\n-db: {dbLatencyString}\r\n");
            sb.Append($"-gateway: {discordLatency}\r\n");
            sb.Append($"### Info: \r\n[Murdox](https://top.gg/bot/991070265578516561)\r\n-shards: {gatewayInfo.ShardCount}\r\n-guilds: {guildCount}");

            DiscordComponent[] components =
              [
                  new DiscordTextDisplayComponent($"# Ping"),
                  new DiscordSeparatorComponent(true, DiscordSeparatorSpacing.Large),
                  new DiscordTextDisplayComponent(sb.ToString()),
                  new DiscordSeparatorComponent(true),
                  new DiscordSectionComponent(new DiscordTextDisplayComponent($"-# Murdox ©️ {DateTime.UtcNow:ddd, MM-dd-yyyy hh:mm tt}"),
                        new DiscordButtonComponent(DiscordButtonStyle.Secondary, "donateBtn", "Donate")),
              ];
            var container = new DiscordContainerComponent(components, false, DiscordColor.Azure);

            var msg = new DiscordMessageBuilder()
                .EnableV2Components()
                .AddContainerComponent(container);

            await ctx.EditResponseAsync(msg);
        }
        #endregion

        #region ENABLE DAILY RANDOM FACTS
        [Command("enable-facts")]
        [Description("Enable daily random facts in the server")]
        public async Task EnableFacts(CommandContext ctx)
        {
            await ctx.DeferResponseAsync();
            var provider = ctx.Client.ServiceProvider;
            var scheduler = provider.GetRequiredService<IScheduler>();

            using var db = _dbFactory.CreateDbContext();
            var guild = await db.Guilds
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.GuildId == ctx.Guild!.Id.ToString());
            if (guild is null)
            {
                var g = new Server()
                {
                    GuildId = ctx.Guild!.Id.ToString(),
                    GuildName = ctx.Guild.Name,
                    EnableFacts = true,
                    NotificationChannelId = ctx.Guild.GetDefaultChannel()!.Id.ToString(),
                    OwnerId = ctx.Guild.OwnerId.ToString(),
                    OwnerUsername = ctx.Guild.GetMemberAsync(ctx.Guild.OwnerId).Result.GlobalName!,
                    CreatedAt = DateTimeOffset.UtcNow,
                };
                await db.Guilds.AddAsync(g);
                await db.SaveChangesAsync();
            }
            await ctx.RespondAsync("facts are enabled!");
        }
        #endregion
    }
}
