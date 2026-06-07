using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using MurdoxV2.Features.ScamDetection;
using MurdoxV2.Interfaces;
using MurdoxV2.Models;
using MurdoxV2.Services.MessageCache;
using MurdoxV2.Services.Tags;
using SkiaSharp;
using System.Collections.Concurrent;
using static System.Net.WebRequestMethods;

namespace MurdoxV2.Handlers
{
    public class MessageCreatedEventHandler(ITagRepository tagRepository, TagEmbedBuilderProviderService tagEmbedBuilder,
        IUrlCaptureService captureService, IUrlRemovaleService removalService, DiscordMessageCacheService msgCache,
        IScamDetectionService scamService, HttpClient client, ILogger<MessageCreatedEventHandler> logger): IEventHandler<MessageCreatedEventArgs>
    {
        private readonly IScamDetectionService _scamService = scamService;
        private readonly HttpClient _http = client;
        private readonly ILogger<MessageCreatedEventHandler> _logger = logger;
        public async Task HandleEventAsync(DiscordClient sender, MessageCreatedEventArgs eventArgs)
        {
            if (eventArgs.Author.IsBot) return;

            var ct = new CancellationTokenSource().Token;
            var cachedMsg = new CachedDiscordMessage
            {
                MessageId = eventArgs.Message.Id,
                AuthorId = eventArgs.Message.Author!.Id,
                ChannelId = eventArgs.Message.ChannelId,
                Content = eventArgs.Message.Content,
                MentionedUserIds = [.. eventArgs.MentionedUsers.Select(x => x.Id)],
            };
            msgCache.Set(cachedMsg);

           if (eventArgs.Message.Content.StartsWith('$'))
           {
                var msg = eventArgs.Message.Content[1..];
                
                switch (msg)
                {
                    case "itdepends":
                        var tag = await tagRepository.GetByNameAsync(eventArgs.Guild.Id, "itDepends", ct) ?? 
                            new Tag 
                            { 
                                GuildId = eventArgs.Guild.Id,
                                Name = "ItDepends",
                                Content = "Unknown Tag - this tag is not created yet or the name was changed",
                                CreatedAt = DateTimeOffset.UtcNow,
                                LastModifiedAt = DateTimeOffset.UtcNow,
                                LastModifiedBy = null,
                            };
                        var tagEmbed = tagEmbedBuilder.BuildTagEmbed(tag, false, DiscordColor.Blurple);
                        await eventArgs.Channel.SendMessageAsync(tagEmbed);
                        break;
                    case "scoreshelp":

                        break;
                    case "dailynumbers":

                        break;
                    default:
                        await eventArgs.Channel.SendMessageAsync("UNKOWN TAG");
                        break;
                }
           }
           else
           {
                var urls = captureService.Capture(eventArgs.Message.Content);
                if (urls.Count > 0)
                {
                    string cleaned = removalService.RemoveUrls(eventArgs.Message.Content, true);
                    await eventArgs.Message.DeleteAsync();
                    await eventArgs.Channel.SendMessageAsync(cleaned);
                }

                if (eventArgs.Message.Attachments.Count > 0)
                {
                    foreach (var attachment in eventArgs.Message.Attachments)
                    {
                        if (!IsImage(attachment.FileName!))
                            continue;

                        // Download raw bytes
                        var bytes = await _http.GetByteArrayAsync(attachment.Url);

                        // Decode bitmap for hashing
                        using var bitmap = SKBitmap.Decode(bytes);
                        if (bitmap is null)
                            continue;

                        // Build your primary‑constructor context
                        var context = new ScamImageContext(
                            eventArgs.Guild,
                            eventArgs.Channel,
                            eventArgs.Message,
                            attachment,
                            eventArgs.Author
                        );

                        // Call the updated service signature
                        var result = await _scamService.AnalyzeAsync(context, bytes);

                        if (result.IsActionable)
                        {
                            await eventArgs.Message.DeleteAsync();
                            await Task.Delay(1000);
                            await eventArgs.Channel.SendMessageAsync(
                                $"{eventArgs.Author.Mention} your image was removed. Reason: {result.Reason}");

                            _logger.LogInformation("known scam image detected in guild [{GuildId}]: Name [{GuildName}]: by user {UserId}. Reason: {Reason}",
                                eventArgs.Guild.Id, eventArgs.Guild.Name, eventArgs.Author.Id, result.Reason);
                        }
                    }

                }
            }
        }

        private static bool IsImage(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext is ".png" or ".jpg" or ".jpeg" or ".gif" or ".webp";
        }
    }
}
