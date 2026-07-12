using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MurdoxV2.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Handlers.Button
{
    public sealed class PaginationButtonHandler : IButtonHandler
    {
        public bool CanHandle(string customId)
         => customId.Equals("next") || customId.Equals("previous");

        public async Task HandleAsync(ComponentInteractionCreatedEventArgs e)
        {
            bool isNext = e.Id == "next";
            await PaginationService.UpdateAsync(e, isNext);
        }
    }
}
