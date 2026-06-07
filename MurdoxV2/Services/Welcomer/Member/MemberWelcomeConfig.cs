using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MurdoxV2.Services.Welcomer.Member
{
    public sealed class MemberWelcomeConfig
    {
        [JsonPropertyName("memberWelcomeMessages")]
        public required IReadOnlyList<MemberWelcomeMessage> MemberWelcomeMessages { get; init; }
    }
}
