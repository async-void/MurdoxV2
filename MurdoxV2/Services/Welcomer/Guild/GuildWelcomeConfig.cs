using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MurdoxV2.Services.Welcomer.Guild
{
    public sealed class GuildWelcomeConfig
    {
        [JsonPropertyName("guildWelcomeMessages")]
        public required IReadOnlyList<GuildWelcomeMessage> GuildWelcomeMessages { get; init; }
    }
}
