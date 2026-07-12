using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MurdoxV2.Handlers.Modal;

namespace MurdoxV2.Handlers;

public class InteractionEventHandler(ModalRouter modalRouter) : IEventHandler<ModalSubmittedEventArgs>
{
    public async Task HandleEventAsync(DiscordClient client, ModalSubmittedEventArgs eventArgs)
    {

        if (eventArgs.Interaction.Type != DiscordInteractionType.ModalSubmit) return;

        switch (eventArgs.Interaction.Type)
        {
            #region MODALS
            case DiscordInteractionType.ModalSubmit:
                var modalId = eventArgs.Interaction.Data.CustomId;
                if (modalRouter.TryGetHandler(modalId, out var handler))
                {
                    await handler.HandleAsync(eventArgs);
                    return;
                }

                await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent($"No modal handler found for ID: {modalId} TYPE: [{handler.GetType()}")
                        .AsEphemeral());
                break;
            #endregion

            #region SELECT MENUS
            case DiscordInteractionType.ApplicationCommand:
                break;
            #endregion

            default:
                DiscordComponent[] defaultComp =
                [
                new DiscordTextDisplayComponent($"## Uknown Interaction Type"),
                new DiscordSeparatorComponent(true),
                new DiscordTextDisplayComponent($"[Interaction.Type] Un-Recognized"),
                new DiscordSeparatorComponent(true, DiscordSeparatorSpacing.Large),
                new DiscordSectionComponent(new DiscordTextDisplayComponent("Donatations help to keep Murdox online!"),
                new DiscordButtonComponent(DiscordButtonStyle.Secondary, "donateBtn", "Donate"))
                ];
                DiscordContainerComponent default_container = new DiscordContainerComponent(defaultComp, false, DiscordColor.Magenta);

                var defaultMsg = new DiscordInteractionResponseBuilder()
                .EnableV2Components()
                .AddContainerComponent(default_container);
                await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(defaultMsg));
                break;
        }
    }
}
