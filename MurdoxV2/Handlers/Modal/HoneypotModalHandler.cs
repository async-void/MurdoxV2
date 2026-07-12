using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace MurdoxV2.Handlers.Modal;

public sealed class HoneypotModalHandler : IModalHandler
{
    public string CustomId => "honeypot:set";

    public async Task HandleAsync(ModalSubmittedEventArgs e)
    {
        var channelName = ((TextInputModalSubmission)e.Values["channelName"]).Value;
        var guild = e.Interaction.Guild;
        var channel = await guild.CreateChannelAsync(channelName, DiscordChannelType.Text, null, "do not post here", null, null, null, null,
            null, null, 0);

        await e.Interaction.CreateResponseAsync(
        DiscordInteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
                .WithContent($"Honeypot channel created: {e.Interaction.Message.Author.Mention}")
                .AsEphemeral());
    }
}