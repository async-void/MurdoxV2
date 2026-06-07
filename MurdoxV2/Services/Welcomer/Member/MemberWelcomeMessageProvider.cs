using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Services.Welcomer.Member
{
    public sealed class MemberWelcomeMessageProvider(MemberWelcomeConfig config)
    {
        public IReadOnlyList<MemberWelcomeMessage> MemberWelcomeMessages { get; } = config.MemberWelcomeMessages;
    }
}
