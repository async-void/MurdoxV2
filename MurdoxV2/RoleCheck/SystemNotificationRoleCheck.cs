using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace MurdoxV2.RoleCheck
{
    public class SystemNotificationRoleCheck : IContextCheck<SysteNotificationRoleAttribute>
    {
       
        public ValueTask<string?> ExecuteCheckAsync(SysteNotificationRoleAttribute attribute, CommandContext ctx)
        {
            var hasRole = ctx.Member.Roles.Any(r => r.Id == 1501498012822933520);

            if (hasRole) return ValueTask.FromResult<string?>(null);

            return ValueTask.FromResult<string?>("You do not have the required role to use this command.");
        }
    }
}

