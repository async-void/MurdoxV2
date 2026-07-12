using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Models
{
    public record DiceRollerPayload(
        Guid Identifier,
        string DisplayName,
        string DiceExpression,
        IReadOnlyList<int> Roll,
        int Total,
        string? Message = null
    );
    
}
