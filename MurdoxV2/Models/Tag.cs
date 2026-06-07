using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public required ulong GuildId { get; init; }
        public ulong  TagId { get; init; }
        public required string Name { get; init; }
        public required string Content { get; set; }
        public ulong Author { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public ulong? LastModifiedBy { get; set; }
        public DateTimeOffset LastModifiedAt { get; set; }
        public List<string>? TagLinks { get; set; }
    }
}
