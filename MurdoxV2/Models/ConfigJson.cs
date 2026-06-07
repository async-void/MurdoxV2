using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MurdoxV2.Models
{
    public class ConfigJson
    {
        [JsonPropertyName("Discord")]
        public DiscordConfig? Discord { get; set; }
        [JsonPropertyName("ConnectionStrings")]
        public ConnectionStrings? ConnectionStrings { get; set; }
    }
}
