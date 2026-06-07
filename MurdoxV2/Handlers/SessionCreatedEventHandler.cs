using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Handlers
{
    public class SessionCreatedEventHandler(ILogger<SessionCreatedEventHandler> logger) : IEventHandler<SessionCreatedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient sender, SessionCreatedEventArgs eventArgs)
        {
            logger.LogInformation($"Discord Session Created...");
        }
    }
}
