using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using MurdoxV2.Data.DbContext;
using MurdoxV2.Factories;
using MurdoxV2.Utilities.Timestamp;
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
    public class UtilityCommands(IDbContextFactory<AppDbContext> dbFactory)
    {
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

        [Command("ping")]
        [Description("get latency info for the system")]
        public async Task PingAsync(CommandContext ctx)
        {
            await ctx.DeferResponseAsync();
            var sw = new Stopwatch();
            var guildId = ctx.Guild?.Id ?? 0;
            var discordPing = ctx.Client.GetConnectionLatency(guildId);

            sw.Start();
            var dbLatency = dbFactory.CreateDbContext();
            sw.Stop();
            var dbPing = sw.ElapsedMilliseconds;

            sw.Restart();
            var gatewayInfo = await ctx.Client.GetGatewayInfoAsync();
            var gatewayLatency = sw.ElapsedMilliseconds;
            sw.Stop();

            var dbLatencyString = dbPing > 1000 ? $"{dbPing / 1000.0:F2} seconds" : $"{dbPing} ms";
            var discordLatency = gatewayLatency > 1000 ? $"{discordPing / 1000.0:F2} seconds" : $"{discordPing} ms";

            DiscordComponent[] components =
              [
                  new DiscordTextDisplayComponent($"# Ping"),
                  new DiscordSeparatorComponent(true, DiscordSeparatorSpacing.Large),
                  new DiscordTextDisplayComponent($"Database Latency: {dbLatencyString}"),
                  new DiscordTextDisplayComponent($"Gateway Latency: {gatewayLatency} ms"),
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
    }
}
