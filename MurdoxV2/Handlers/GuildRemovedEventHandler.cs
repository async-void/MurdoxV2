using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using MurdoxV2.Factories;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Handlers
{
    public class GuildRemovedEventHandler(AppDbContextFactory dbFactory, ILogger<GuildRemovedEventHandler> logger) : IEventHandler<GuildDeletedEventArgs>
    {
        public Task HandleEventAsync(DiscordClient sender, GuildDeletedEventArgs eventArgs)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss.fff tt zzz");
            var db = dbFactory.CreateDbContext();
            logger.LogInformation("Guild Removed: {name} ({id})", eventArgs.Guild.Name, eventArgs.Guild.Id);
            return Task.CompletedTask;
        }
    }
}
