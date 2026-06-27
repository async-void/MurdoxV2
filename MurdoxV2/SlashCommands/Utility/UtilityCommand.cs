using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MurdoxV2.Data.DbContext;
using MurdoxV2.Extensions;
using MurdoxV2.Interfaces;
using MurdoxV2.Models;
using MurdoxV2.Utilities.Timestamp;
using Quartz;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace MurdoxV2.SlashCommands.Utility;

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
    [RequirePermissions(DiscordPermission.ManageChannels)]
    public async Task EnableFacts(CommandContext ctx)
    {
        await ctx.DeferResponseAsync();
        var provider = ctx.Client.ServiceProvider;
        var scheduler = provider.GetRequiredService<IScheduler>();

        using var db = _dbFactory.CreateDbContext();
        var guild = await db.Guilds
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.GuildId == ctx.Guild!.Id);
        if (guild is null)
        {
            var g = new Server()
            {
                GuildId = ctx.Guild!.Id,
                GuildName = ctx.Guild.Name,
                EnableFacts = true,
                NotificationChannelId = ctx.Guild.GetDefaultChannel()!.Id,
                OwnerId = ctx.Guild.OwnerId,
                OwnerUsername = ctx.Guild.GetMemberAsync(ctx.Guild.OwnerId).Result.GlobalName!,
                CreatedAt = DateTimeOffset.UtcNow,
            };
            await db.Guilds.AddAsync(g);
            await db.SaveChangesAsync();
        }
        await ctx.RespondAsync("facts are enabled!");
    }
    #endregion

    #region DISABLE FACTS
    [Command("disable-facts")]
    [Description("Disable daily random facts in the server")]
    [RequirePermissions(DiscordPermission.ManageChannels)]
    public async ValueTask DisableFacts(SlashCommandContext ctx)
    {

    }
    #endregion

    #region ABOUT
    [Command("about")]
    [Description("Murdox about section")]
    public async ValueTask MurdoxHelp(SlashCommandContext ctx)
    {
        await ctx.DeferResponseAsync();
        var timestamp = DateTimeOffset.UtcNow.ToTimestamp();
        var uptime = await TimestampDataProvider.GetBotUptimeAsync();
        var guildCount = ctx.Client.Guilds.Count();
        var memoryUsage = 0L;
        using (var proc = Process.GetCurrentProcess())
        {
            memoryUsage = proc.PrivateMemorySize64;
        }
        var rows = new List<(string Label, string Value)>
        {
            ("Uptime".PadRight(10), uptime.Humanize()),
            ("Memory", $"{memoryUsage / (1024 * 1024)} MB"),
            ("Guilds", guildCount.ToString())
        };

        var arrowIcon = "<:right_arrow:1519237138498064484>";

        DiscordComponent[] comps =
        [
            new DiscordTextDisplayComponent("## About ℹ️\r\n-# Murdox is a full service Moderation Discord Bot"),
            new DiscordSeparatorComponent(true),
            new DiscordTextDisplayComponent($"{arrowIcon} {rows[0].Label} {rows[0].Value}"),
            new DiscordTextDisplayComponent($"{arrowIcon} {rows[1].Label} {rows[1].Value,8}"),
            new DiscordTextDisplayComponent($"{arrowIcon} {rows[2].Label} {rows[2].Value,8}"),
            new DiscordSeparatorComponent(true),
            new DiscordSectionComponent(new DiscordTextDisplayComponent($"Murdox ©️ {timestamp}"),
                new DiscordButtonComponent(DiscordButtonStyle.Secondary, "donateBtn", "Donate"))
        ];
        var container = new DiscordContainerComponent(comps, false, DiscordColor.Goldenrod);
        var msg = new DiscordMessageBuilder()
            .EnableV2Components()
            .AddContainerComponent(container);
        await ctx.RespondAsync(msg);
    }

    #endregion
}
