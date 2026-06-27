using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Handlers.Button
{
    public readonly record struct ButtonId(string Value)
    {
        public override string ToString() => Value;
    }
}
