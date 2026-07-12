using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Handlers.Modal
{
    public abstract class ModalHandlerBase(string customId): IModalHandler
    {
        public string CustomId => customId;

        protected string GetValue(InteractionCreatedEventArgs e, string id) 
        {
            return e.Interaction.Data.Components!
                .OfType<DiscordTextInputComponent>()
                .First(c => c.CustomId == id)
                .Value!;
        }

        public abstract Task HandleAsync(ModalSubmittedEventArgs e);
    }
}
