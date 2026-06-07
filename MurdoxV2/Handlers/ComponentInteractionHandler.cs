using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MurdoxV2.Pagination;


namespace MurdoxV2.Handlers
{
    public class ComponentInteractionHandler : IEventHandler<ComponentInteractionCreatedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient sender, ComponentInteractionCreatedEventArgs e)
        {
            var componentType = e.Interaction.Data.ComponentType;
            switch (componentType)
            {
                case DiscordComponentType.Button:
                    switch (e.Id)
                    {
                        case "next":
                        case "previous":
                            bool isNext = e.Id == "next";
                            await PaginationService.UpdateAsync(e, isNext);
                            break;
                        case "donateBtn":
                            var donateUrl = "https://www.buymeacoffee.com/murdox";
                            var embed = new DiscordEmbedBuilder
                            {
                                Title = "Support Murdox",
                                Description = $"If you enjoy using Murdox, consider supporting its development by buying me a coffee! [Donate here]({donateUrl}) ☕",
                                Color = DiscordColor.Gold
                            };
                            await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed).AsEphemeral());
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
                   
            }
        }
    }
}
