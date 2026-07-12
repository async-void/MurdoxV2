using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MurdoxV2.Features.AutoLearning;

namespace MurdoxV2.Handlers.Button;

public class AutoLearnButtonHandler(AutoLearningService autoLearning) : IButtonHandler
{
    private readonly AutoLearningService _autoLearning = autoLearning;

    public bool CanHandle(string customId)
        => customId.StartsWith("autolearn:");

    public async Task HandleAsync(ComponentInteractionCreatedEventArgs e)
    {
        var parts = e.Id.Split(':');
        var key = parts[1];
        var action = parts[2];

        if (!AutoLearningCache.TryGet(key, out var pending))
            return;

        if (action == "confirm")
        {
            await _autoLearning.AddNewScamImage(pending.Bytes, pending.Analysis);
            var pendingUrl = pending.Analysis.Context.Attachment.Url;
            DiscordComponent[] comps =
            [
                new DiscordTextDisplayComponent("## SCAM IMAGE"),
                                new DiscordSeparatorComponent(true),
                                new DiscordTextDisplayComponent("✅ Added to AutoLearning database"),
                                new DiscordMediaGalleryComponent(new DiscordMediaGalleryItem(pendingUrl))
            ];
            var container = new DiscordContainerComponent(comps, false, DiscordColor.DarkRed);
            await e.Interaction.CreateResponseAsync(
                             DiscordInteractionResponseType.UpdateMessage,
                             new DiscordInteractionResponseBuilder()
                                 .EnableV2Components()
                                 .AddContainerComponent(container));

        }
        else
        {
            var pendingUrl = pending.Analysis.Context.Attachment.Url;
            DiscordComponent[] comps =
            [
                new DiscordTextDisplayComponent("## NOT SCAM IMAGE"),
                                new DiscordSeparatorComponent(true),
                                new DiscordTextDisplayComponent("✅ Mod Determined [NOT SCAM]"),
                                new DiscordMediaGalleryComponent(new DiscordMediaGalleryItem(pendingUrl))
            ];
            var container = new DiscordContainerComponent(comps, false, DiscordColor.Teal);
            await e.Interaction.CreateResponseAsync(
                             DiscordInteractionResponseType.UpdateMessage,
                             new DiscordInteractionResponseBuilder()
                                 .EnableV2Components()
                                 .AddContainerComponent(container));
        }

        AutoLearningCache.Remove(key);
    }
}
