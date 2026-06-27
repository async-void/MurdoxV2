using MurdoxV2.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Features.ScamDetection
{
    public sealed class ScamAnalysisResult(ScamVerdict verdict, string reason, float score)
    {
        public ScamVerdict Verdict { get; } = verdict;
        public string Reason { get; } = reason;
        public float Score { get; } = score;

        public ScamImageRecord? Match { get; init; }
        public ScamImageContext? Context { get; init; }

        public bool IsActionable => Verdict == ScamVerdict.Scam;
    }
}
