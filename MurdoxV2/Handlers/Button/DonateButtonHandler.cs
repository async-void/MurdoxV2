using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MurdoxV2.Interfaces;

namespace MurdoxV2.Handlers.Button
{
    public sealed class DonateButtonHandler : IButtonHandler
    {
        public bool CanHandle(string customId)
         => customId.Equals("donateBtn");

        public async Task HandleAsync(ComponentInteractionCreatedEventArgs e)
        {
            var donateUrl = "https://www.buymeacoffee.com/murdox";
            var embed = new DiscordEmbedBuilder
            {
                Title = "Support Murdox",
                Description = $"If you enjoy using Murdox, consider supporting its development by buying me a coffee! [Donate here]({donateUrl}) ☕",
                Color = DiscordColor.Gold
            };
            await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, 
                new DiscordInteractionResponseBuilder().AddEmbed(embed).AsEphemeral());
        }
    }
}
