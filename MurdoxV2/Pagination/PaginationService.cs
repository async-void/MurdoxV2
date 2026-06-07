using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace MurdoxV2.Pagination
{
    public sealed class PaginationService
    {
        #region SEND PAGINATED MESSAGE
        public static async Task<DiscordMessage> SendPaginatedMessageAsync(DiscordChannel channel, IReadOnlyList<DiscordContainerComponent> pages, string contextType)
        {
           var ctx = new PaginationContext<DiscordContainerComponent>(pages, 0, contextType);
           var builder = new DiscordMessageBuilder()
                .AddContainerComponent(pages[0])
                .AddActionRowComponent(BuildButtonRow(ctx))
                .EnableV2Components();

            var message = await builder.SendAsync(channel);
            PaginationRegistry.Register(message.Id, ctx);
            return message;
        }
        #endregion

        #region UPDATE MESSAGE
        public static async Task UpdateAsync(ComponentInteractionCreatedEventArgs e, bool isNext)
        {
            var msgId = e.Message.Id;
            var ctx = PaginationRegistry.Get<DiscordContainerComponent>(msgId);

            if (ctx is null)
            {
                await e.Interaction.CreateFollowupMessageAsync(
                    new DiscordFollowupMessageBuilder().WithContent("Paging expired.")
                );
                return;
            }

            await e.Interaction.CreateResponseAsync(
               DiscordInteractionResponseType.DeferredMessageUpdate
           );
            ctx.CurrentIndex += isNext ? 1 : -1;
            ctx.CurrentIndex = Math.Clamp(ctx.CurrentIndex, 0, ctx.Pages.Count - 1);

            var builder = new DiscordWebhookBuilder()
               .AddContainerComponent(ctx.Pages[ctx.CurrentIndex])
               .AddActionRowComponent(BuildButtonRow(ctx))
               .EnableV2Components();

            // Now update the original message
            await e.Interaction.EditOriginalResponseAsync(builder);

        }
        #endregion

        #region BUILD BUTTON ROW
        public static DiscordActionRowComponent BuildButtonRow(PaginationContext<DiscordContainerComponent> ctx)
        {
            DiscordComponent[] buttons =
            [
                new DiscordButtonComponent(
                    DiscordButtonStyle.Secondary,
                    "previous",
                    "⬅️",
                    !ctx.HasPrevious
                ),
                new DiscordButtonComponent(
                    DiscordButtonStyle.Secondary,
                    "next",
                    "➡️",
                    !ctx.HasNext
                )
            ];
          
            return new DiscordActionRowComponent(buttons);
        }
        #endregion
    }
}
