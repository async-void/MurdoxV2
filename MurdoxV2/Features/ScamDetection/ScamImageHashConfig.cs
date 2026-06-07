using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Features.ScamDetection
{
    public sealed class ScamImageHashConfig(int AHashThreshold = 10, int DHashThreshold = 10, int PHashThreshold = 12)
    {
        public int AHashThreshold { get; } = AHashThreshold;
        public int DHashThreshold { get; } = DHashThreshold;
        public int PHashThreshold { get; } = PHashThreshold;
    }

}
