using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Interfaces
{
    public interface IUrlCaptureService
    {
        IReadOnlyList<string> Capture(string text);
    }
}
