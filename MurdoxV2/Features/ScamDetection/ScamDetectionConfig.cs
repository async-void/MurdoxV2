using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Features.ScamDetection
{
    public sealed class ScamDetectionConfig
    {
        public float ScamThreshold { get; } = 0.85f;
        public float SuspiciousThreshold { get; } = 0.65f;
        public IReadOnlyCollection<string> WhitelistedDomains { get; init; } = [];
        public IReadOnlyCollection<ulong> WhitelistedUserIds { get; } = [];
    }
}
