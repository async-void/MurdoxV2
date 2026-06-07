using DSharpPlus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MurdoxV2.Data.DbContext;
using MurdoxV2.Features.ScamDetection;
using MurdoxV2.Hashing;
using Serilog;
using SkiaSharp;

namespace MurdoxV2.Services
{
    public class BotService(ILogger<BotService> logger, DiscordClient client, IDbContextFactory<AppDbContext> dbFactory, IScamHashRepository repo) : IHostedService
    {
        private readonly ILogger<BotService> _logger = logger;
       //private readonly IHostApplicationLifetime _appLifetime = appLifetime;
        private readonly DiscordClient _dClient = client;
        private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(BotService)} started");
            _logger.LogInformation("Connecting to Discord...");
            var db = _dbFactory.CreateDbContext();
            _logger.LogInformation("running database migrations...");
            await db.Database.MigrateAsync(cancellationToken: cancellationToken);
            _logger.LogInformation("database migration complete!");
            //await SeedScamImage(repo);
            await _dClient.ConnectAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Disconnecting from Discord...");
            await _dClient.DisconnectAsync();
        }

        public static async Task SeedScamImage(IScamHashRepository repo)
        {
            var files = Directory.GetFiles("SeedImages");
            var bytes = File.ReadAllBytes("crypto.png");
            using var bitmap = SKBitmap.Decode(bytes);

            var record = new ScamImageRecord
            {
                Id = Guid.NewGuid(),
                AHash = ImageHashing.AverageHash(bitmap),
                DHash = ImageHashing.DifferenceHash(bitmap),
                PHash = ImageHashing.PerceptualHash(bitmap),
                Category = "Test",
                Description = "Synthetic crypto giveaway",
                CreatedAt = DateTimeOffset.UtcNow,
                AddedBy = "Gene"
            };

            await repo.AddAsync(record);
        }

    }
}
