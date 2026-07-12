using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;

namespace MurdoxV2.RoleCheck
{
    public sealed class ModerationRoleCheck : IContextCheck<ModerationRoleAttribute>
    {
        public ValueTask<string?> ExecuteCheckAsync(ModerationRoleAttribute attribute, CommandContext ctx)
        {
            var callingMember = ctx.Member ?? throw new ArgumentNullException(nameof(ctx.Member));
            var hasPermission = callingMember.Permissions.HasFlag(attribute.AllowedPermission);

            if (hasPermission) return ValueTask.FromResult<string?>(null);

            return ValueTask.FromResult<string?>("You do not have the required permission to use this command.");
        }
    }
}
