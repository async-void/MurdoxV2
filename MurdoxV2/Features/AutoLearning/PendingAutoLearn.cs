using MurdoxV2.Features.ScamDetection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Features.AutoLearning
{
    public class PendingAutoLearn
    {
        public byte[] Bytes { get; }
        public ScamAnalysisResult Analysis { get; }

        public PendingAutoLearn(byte[] bytes, ScamAnalysisResult analysis)
        {
            Bytes = bytes;
            Analysis = analysis;
        }
    }

}
