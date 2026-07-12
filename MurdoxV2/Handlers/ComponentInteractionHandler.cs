using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MurdoxV2.Handlers.Button;


namespace MurdoxV2.Handlers;

public class ComponentInteractionHandler(ButtonRouter router) : IEventHandler<ComponentInteractionCreatedEventArgs>
{
    public async Task HandleEventAsync(DiscordClient sender, ComponentInteractionCreatedEventArgs e)
    {
        var componentType = e.Interaction.Data.ComponentType;
        if (componentType != DiscordComponentType.Button)
            return;

        var customId = e.Interaction.Data.CustomId;

        if (router.TryGetHandler(customId, out var handler))
        {
            await handler.HandleAsync(e);
            return;
        }
        else
        {
            await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent($"No handler for {customId} found"));
            return;
        }
    }
}
