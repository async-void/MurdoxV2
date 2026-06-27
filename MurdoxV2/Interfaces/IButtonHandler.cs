using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MurdoxV2.Handlers.Button;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Interfaces
{
    public interface IButtonHandler
    {
        bool CanHandle(string customId);
        Task HandleAsync(ComponentInteractionCreatedEventArgs e);
    }
}
