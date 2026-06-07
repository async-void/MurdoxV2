using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Handlers
{
    public class SessionResumedEventHandler(ILogger<SessionResumedEventHandler> logger) : IEventHandler<SessionResumedEventArgs>
    {
        public async Task HandleEventAsync(DiscordClient sender, SessionResumedEventArgs eventArgs)
        {
            logger.LogInformation("Discord Session Resumed...");
        }
    }
}
