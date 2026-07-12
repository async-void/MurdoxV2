using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace MurdoxV2.Handlers.Button;

public sealed class ConfigureButtonHandler : IButtonHandler
{
    public bool CanHandle(string customId)
     => customId.StartsWith("configure:");

    public async Task HandleAsync(ComponentInteractionCreatedEventArgs e)
    {
        var action = e.Id.Split(":")[1];

        switch(action)
        {
            case "honeypot":
                var honeyPotModel = new DiscordModalBuilder()
                    .WithCustomId("honeypot:set")
                    .WithTitle("Configure Honeypot Channel")
                    .AddTextInput(new DiscordTextInputComponent("channelName", "honeypot channel name"), "Channel Name", "Name to be used for the Channel");

                await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, honeyPotModel);
                break;
            case "facts":

                break;
            default:

                break;
        }
    }
}
