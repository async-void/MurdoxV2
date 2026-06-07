using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MurdoxV2.Services.Welcomer.Member
{
    public sealed class MemberWelcomeMessage
    {
        [JsonPropertyName("emoji")]
        public required string Emoji { get; set; }
        [JsonPropertyName("message")]
        public required string Message { get; set; }
    }
}
