using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MurdoxV2.Utilities.Timestamp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Handlers
{
    public class InteractionEventHandler : IEventHandler<InteractionCreatedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient client, InteractionCreatedEventArgs eventArgs)
        {
            switch (eventArgs.Interaction.Type)
            {
                case DiscordInteractionType.Component:
                    switch (eventArgs.Interaction.Data.CustomId)
                    {
                        case "purgeBtn":
                            // Handle purge button interaction
                            DiscordComponent[] components =
                               [
                                   new DiscordTextDisplayComponent($"**Purge Command**"),
                                   new DiscordSeparatorComponent(true),
                                   new DiscordTextDisplayComponent("this command will delete a set amount of messages from the chat history."),
                                   new DiscordSeparatorComponent(true, DiscordSeparatorSpacing.Large),
                                   new DiscordSectionComponent(new DiscordTextDisplayComponent("Donatations help to keep Murdox online!"), 
                                        new DiscordButtonComponent(DiscordButtonStyle.Secondary, "donateBtn", "Donate"))
                               ];
                            var container = new DiscordContainerComponent(components, false, DiscordColor.DarkGray);

                            var message = new DiscordInteractionResponseBuilder()
                            .EnableV2Components()
                            .AddContainerComponent(container);

                            await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(message));
                            break;
                        case "banBtn":
                            // Handle ban button interaction
                            await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder()
                                .WithContent("Ban button clicked!"));
                            break;
                        case "warnBtn":
                            // Handle warn button interaction
                            await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder()
                                .WithContent("Warn button clicked!"));
                            break;
                        case "donateBtn":
                            // Handle warn button interaction
                            await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder()
                                .WithContent("Donate button clicked!"));
                            break;
                        case "uptimeBtn":
                            // Handle warn button interaction
                            var uptime = await TimestampDataProvider.GetBotUptimeAsync();
                            components =
                              [
                                  new DiscordTextDisplayComponent($"## Uptime Command"),
                                  new DiscordSeparatorComponent(true),
                                  new DiscordTextDisplayComponent($"Murdox has been online for: {uptime}"),
                                  new DiscordSeparatorComponent(true, DiscordSeparatorSpacing.Large),
                                  new DiscordSectionComponent(new DiscordTextDisplayComponent("Donatations help to keep Murdox online!"),
                                        new DiscordButtonComponent(DiscordButtonStyle.Secondary, "donateBtn", "Donate"))
                              ];
                            container = new DiscordContainerComponent(components, false, DiscordColor.Magenta);

                            message = new DiscordInteractionResponseBuilder()
                            .EnableV2Components()
                            .AddContainerComponent(container);

                            await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(message));
                            break;
                        default:
                            await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder()
                                .WithContent("Unknown button interaction."));
                            break;
                    }
                    break;
                case DiscordInteractionType.ApplicationCommand:
                    
                default:
                   // await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder()
                       // .WithContent("This interaction type is not yet implemented."));
                    break;
            }
        }
    }
}
