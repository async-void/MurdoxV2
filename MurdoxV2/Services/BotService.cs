using DSharpPlus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MurdoxV2.Data.DbContext;
using MurdoxV2.Features.ScamDetection;
using MurdoxV2.Hashing;
using MurdoxV2.Models;
using SkiaSharp;
using System.Text.Json;

namespace MurdoxV2.Services;

public class BotService(ILogger<BotService> logger, DiscordClient client, IDbContextFactory<AppDbContext> dbFactory) : IHostedService
{
    private readonly ILogger<BotService> _logger = logger;
   //private readonly IHostApplicationLifetime _appLifetime = appLifetime;
    private readonly DiscordClient _dClient = client;
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"{nameof(BotService)} started");
        _logger.LogInformation("Connecting to Discord...");
        //await SetPassword();
        var db = _dbFactory.CreateDbContext();
        var pendingMigrations = db.Database.GetPendingMigrations().Any();

        if (pendingMigrations)
        {
            _logger.LogInformation("running database migrations...");
            await db.Database.MigrateAsync(cancellationToken: cancellationToken);
            _logger.LogInformation("database migration complete!");
            
        }
        await _dClient.ConnectAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Disconnecting from Discord...");
        await _dClient.DisconnectAsync();
    }

    #region SET PASSWORD PROPERTY

    private static async Task SetPassword()
    {
        var jsonPath = Path.Combine(AppContext.BaseDirectory, "Data", "Config", "config.json");
        var jsonContent = await File.ReadAllTextAsync(jsonPath);
        var json = JsonSerializer.Deserialize<ConfigJson>(jsonContent);
        var replaced = json.ConnectionStrings!.Murdox!.Replace("_G", "g");
        File.WriteAllText(jsonPath, replaced);

    }

    #endregion
    public static async Task SeedScamImage(IScamHashRepository repo)
    {
        var files = Directory.GetFiles("SeedImages");
        var bytes = File.ReadAllBytes("crypto.png");

        foreach (var file in files)
        {
            var fileBytes = File.ReadAllBytes(file);
            using var bitmap = SKBitmap.Decode(fileBytes);

            var record = new ScamImageRecord
            {
                Id = Guid.NewGuid(),
                AHash = ImageHashing.AverageHash(bitmap),
                DHash = ImageHashing.DifferenceHash(bitmap),
                PHash = ImageHashing.PerceptualHash(bitmap),
                Category = "Seed",
                Description = "Discord Scam Images",
                CreatedAt = DateTimeOffset.UtcNow,
                AddedBy = "Owner"
            };

            await repo.AddAsync(record);
        }
    }

}
