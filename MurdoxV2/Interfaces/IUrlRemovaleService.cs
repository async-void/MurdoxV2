using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Interfaces
{
    public interface IUrlRemovaleService
    {
        string RemoveUrls(string text, bool redact);
    }
}
