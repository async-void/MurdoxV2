using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.RoleCheck
{
    public sealed class ModerationRoleAttribute(DiscordPermission discordPermission) : ContextCheckAttribute
    {
        public DiscordPermission AllowedPermission { get; set; } = discordPermission;
    }
}
