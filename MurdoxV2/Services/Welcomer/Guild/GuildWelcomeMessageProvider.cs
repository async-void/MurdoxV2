using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MurdoxV2.Services.Welcomer.Guild
{
    public sealed class GuildWelcomeMessageProvider(GuildWelcomeConfig config)
    {
        public IReadOnlyList<GuildWelcomeMessage> GuildWelcomeMessages { get; } = config.GuildWelcomeMessages;
    }
}
