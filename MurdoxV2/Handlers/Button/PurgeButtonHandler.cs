using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MurdoxV2.Extensions;

namespace MurdoxV2.Handlers.Button;

public sealed class PurgeButtonHandler : IButtonHandler
{
    public bool CanHandle(string id) =>
        id.Equals("purgeBtn");

    public async Task HandleAsync(ComponentInteractionCreatedEventArgs e)
    {
        var timestamp = DateTimeOffset.UtcNow.ToTimestamp();
        DiscordComponent[] components =
        [
            new DiscordTextDisplayComponent($"## Purge"),
            new DiscordSeparatorComponent(true),
            new DiscordTextDisplayComponent("this command will delete a set amount of messages from the chat history."),
            new DiscordSeparatorComponent(true, DiscordSeparatorSpacing.Large),
            new DiscordSectionComponent(new DiscordTextDisplayComponent($"-# Murdox ©️ {timestamp}"),
                new DiscordButtonComponent(DiscordButtonStyle.Secondary, "donateBtn", "Donate"))
        ];
        var container = new DiscordContainerComponent(components, false, DiscordColor.DarkGray);

        await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AddContainerComponent(container).AsEphemeral());
    }
}
