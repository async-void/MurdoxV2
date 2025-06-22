using DSharpPlus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MurdoxV2.Data.DbContext;
using Serilog;

namespace MurdoxV2.Services
{
    public class BotService : IHostedService
    {
        private readonly ILogger<BotService> logger;
        private readonly IHostApplicationLifetime appLifetime;
        private readonly DiscordClient dClient;

        public BotService(ILogger<BotService> logger, IHostApplicationLifetime appLifetime, DiscordClient dClient)
        {
            this.logger = logger;
            this.appLifetime = appLifetime;
            this.dClient = dClient;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Log.Information("Connecting to Discord...");
            await dClient.ConnectAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Information("Disconnecting from Discord...");
            await dClient.DisconnectAsync();
        }
    }
}
