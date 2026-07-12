using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;

namespace MurdoxV2.RoleCheck
{
    public class SystemNotificationRoleAttribute(DiscordPermission allowedPermission) : ContextCheckAttribute
    {
        public DiscordPermission AllowedPermission { get; set; } = allowedPermission;
    }
}
