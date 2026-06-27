using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using MurdoxV2.Features.ScamDetection;
using MurdoxV2.Hashing;
using MurdoxV2.Interfaces;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Features.AutoLearning
{
    public class AutoLearningService(
    DiscordClient client,
    IScamHashRepository repo,
    IServerRepository servers)
    {
        private readonly DiscordClient _client = client;
        private readonly IScamHashRepository _repo = repo;
        private readonly IServerRepository _servers = servers;

        public async Task SendAutoLearnPromptAsync(
            ulong guildId,
            byte[] bytes,
            ScamAnalysisResult analysis,
            DiscordMessage originalMessage)
        {
            
            var server = await _servers.GetGuildByIdAsync(guildId);

            if (server is null || server.NotificationChannelId == 0)
                return;

            var channel = await _client.GetChannelAsync(server.NotificationChannelId);

            var id = Guid.NewGuid().ToString("N");

            AutoLearningCache.Add(id, new PendingAutoLearn(bytes, analysis));
            DiscordComponent[] comps =
            [
                new DiscordTextDisplayComponent("⚠️ **Suspicious Image Detected**"),
                new DiscordSeparatorComponent(true),
                new DiscordTextDisplayComponent($"User: {originalMessage.Author.Username}\n" +
                                                $"Score: **{analysis.Score:P0}**\n\n" +
                                                $"Approve to add this image to the scam DB.")
            ];
            DiscordButtonComponent[] btns =
            [
                new DiscordButtonComponent(DiscordButtonStyle.Success, $"autolearn:{id}:confirm", "Approve"),
                new DiscordButtonComponent(DiscordButtonStyle.Danger, $"autolearn:{id}:reject", "Reject")
            ];
            var container = new DiscordContainerComponent(comps, false, DiscordColor.Teal);
            var builder = new DiscordMessageBuilder()
                .EnableV2Components()
                .AddContainerComponent(container)
                .AddActionRowComponent(btns);

            await channel.SendMessageAsync(builder);
        }

        public async Task AddNewScamImage(byte[] bytes, ScamAnalysisResult analysis)
        {
            using var bitmap = SKBitmap.Decode(bytes);

            var record = new ScamImageRecord
            {
                Id = Guid.NewGuid(),
                AHash = ImageHashing.AverageHash(bitmap),
                DHash = ImageHashing.DifferenceHash(bitmap),
                PHash = ImageHashing.PerceptualHash(bitmap),
                Category = "AutoLearned",
                Description = analysis.Reason ?? "Auto-learned variant",
                CreatedAt = DateTime.UtcNow,
                AddedBy = "AutoLearning"
            };

            await _repo.AddAsync(record);
        }
    }

}
