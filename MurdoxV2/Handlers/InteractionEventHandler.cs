using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MurdoxV2.Utilities.Timestamp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
                #region BUTTONS
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

                        #region UPTIME
                        case "uptimeBtn":
                            // Handle warn button interaction
                            var uptime = await TimestampDataProvider.GetBotUptimeAsync();
                            components =
                              [
                                  new DiscordTextDisplayComponent($"# Uptime  \t<:clock:1385915131799801988>"), 
                                  new DiscordSeparatorComponent(true, DiscordSeparatorSpacing.Large),
                                  new DiscordTextDisplayComponent($"Murdox has been online for: {uptime}"),
                                  new DiscordSeparatorComponent(true),
                                  new DiscordSectionComponent(new DiscordTextDisplayComponent("Donatations help to keep Murdox online!"),
                                        new DiscordButtonComponent(DiscordButtonStyle.Secondary, "donateBtn", "Donate")),
                                  new DiscordSeparatorComponent(true),
                                  new DiscordTextDisplayComponent($"-# Murdox ©️ {DateTime.UtcNow:ddd, MM-dd-yyyy hh:mm tt}")
                              ];
                            container = new DiscordContainerComponent(components, false, DiscordColor.Azure);

                            message = new DiscordInteractionResponseBuilder()
                            .EnableV2Components()
                            .AddContainerComponent(container);

                            await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(message));
                            break;
                        #endregion

                        #region PING 
                        case "pingBtn": //TODO: fix me
                            var lifetime = await TimestampDataProvider.GetBotUptimeAsync();
                            components =
                              [
                                  new DiscordTextDisplayComponent($"# Ping Command"),
                                  new DiscordSeparatorComponent(true),
                                  new DiscordTextDisplayComponent($"Ping is a wip, the bot devs are working on this feature."),
                                  new DiscordTextDisplayComponent($"Uptime: {lifetime}"),
                                  new DiscordTextDisplayComponent($"Discord: "),
                                  new DiscordTextDisplayComponent($"DB: "),
                                  new DiscordSeparatorComponent(true, DiscordSeparatorSpacing.Large),
                                  new DiscordSectionComponent(new DiscordTextDisplayComponent("Donatations help to keep Murdox online!"),
                                        new DiscordButtonComponent(DiscordButtonStyle.Secondary, "donateBtn", "Donate")),
                                  new DiscordSeparatorComponent(true),
                                  new DiscordTextDisplayComponent($"-# Murdox ©️ {DateTime.UtcNow:ddd, MM-dd-yyyy hh:mm tt}")
                              ];
                            container = new DiscordContainerComponent(components, false, DiscordColor.Grayple);

                            message = new DiscordInteractionResponseBuilder()
                            .EnableV2Components()
                            .AddContainerComponent(container);
                            await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(message));
                            break;
                        #endregion

                        #region DEFAULT
                        default:
                            components =
                              [
                                  new DiscordTextDisplayComponent($"# Uknown Command"),
                                  new DiscordSeparatorComponent(true),
                                  new DiscordTextDisplayComponent($"I don't recognize this command!"),
                                  new DiscordSeparatorComponent(true, DiscordSeparatorSpacing.Large),
                                  new DiscordSectionComponent(new DiscordTextDisplayComponent("Donatations help to keep Murdox online!"),
                                        new DiscordButtonComponent(DiscordButtonStyle.Secondary, "donateBtn", "Donate"))
                              ];
                            container = new DiscordContainerComponent(components, false, DiscordColor.NotQuiteBlack);

                            message = new DiscordInteractionResponseBuilder()
                            .EnableV2Components()
                            .AddContainerComponent(container);
                            await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(message));
                            break;
                        #endregion
                    }
                    break;
                #endregion

                #region SELECT MENUS
                case DiscordInteractionType.ApplicationCommand:
                    break;
                #endregion

                default:
                    DiscordComponent[] defaultComp =
                    [
                    new DiscordTextDisplayComponent($"## Uknown Command"),
                    new DiscordSeparatorComponent(true),
                    new DiscordTextDisplayComponent($"Command Un-Recognized"),
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
}
