using DSharpPlus;
using Microsoft.EntityFrameworkCore;
using MurdoxV2.Data.DbContext;
using MurdoxV2.Interfaces;
using Quartz;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Services
{
    public class DailyFactJob(IDbContextFactory<AppDbContext> dbFactory, DiscordClient client, IFact factService) : IJob
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
        private readonly DiscordClient _client = client;
        private readonly IFact _factService = factService;
        public async Task Execute(IJobExecutionContext context)
        {
            using var db = _dbFactory.CreateDbContext();
            var guilds = db.Guilds.Where(g => g.EnableFacts)
                .ToList();
            if (guilds is null || guilds.Count < 1)
            {
                return;
            }
            var fact = await _factService.GetRandomFactAsync();
            if (fact.IsOk)
            {
                var factCategory = fact.Value.Category;
                var factContent = fact.Value.Content;
                var factUrl = fact.Value.FactUrl;
                foreach (var g in guilds)
                {
                    try
                    {
                        var gId = ulong.Parse(g.GuildId);
                        var channelId = ulong.Parse(g.NotificationChannelId);
                        var channel = await _client.GetChannelAsync(channelId);
                        await _client.SendMessageAsync(channel, $"**Daily Fact:**\n**Category:** {factCategory}\n**Fact:** {factContent}\n[Read more]({factUrl})");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Error sending daily fact to guild {g.GuildName}");
                    }
                }
            }
            var test = context.Scheduler.CheckExists(context.JobDetail.Key);
        }
    }
}
