using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Features.ScamDetection
{
    public interface IImageIngestionService
    {
        Task<ImageIngestionResult> FetchAsync(ScamImageContext context);
    }
}
