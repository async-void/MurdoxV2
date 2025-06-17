using DSharpPlus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
            await dClient.ConnectAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await dClient.DisconnectAsync();
        }
    }
}
