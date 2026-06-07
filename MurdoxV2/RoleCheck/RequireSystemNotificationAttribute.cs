using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace MurdoxV2.RoleCheck
{
    public class SysteNotificationRoleAttribute(ulong allowedRole) : ContextCheckAttribute
    {
        public ulong AllowedRole { get; set; } = allowedRole;
    }
}
