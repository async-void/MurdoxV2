using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Features.ScamDetection
{
    public sealed class ScamImageRecord
    {
        public Guid Id { get; init; }

        // 64-bit perceptual hashes
        public long AHash { get; init; }
        public long DHash { get; init; }
        public long PHash { get; init; }

        // Optional metadata
        public string Category { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string? ImageUrl { get; init; }

        public DateTimeOffset CreatedAt { get; init; }
        public string AddedBy { get; init; } = string.Empty; 
        
    }
}
