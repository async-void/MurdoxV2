using DSharpPlus.Entities;

namespace MurdoxV2.Services.Authorization
{
    public sealed class RoleAuthorizationService(IEnumerable<ulong> authorizedRoles, bool allowAdmins = true)
    {
        private readonly HashSet<ulong> _authorizedRoles = [.. authorizedRoles];
        public IReadOnlyCollection<ulong> AuthorizedRoles => _authorizedRoles;
        public bool AddAuthorizedRole(ulong roleId) => _authorizedRoles.Add(roleId);
        public bool RemoveAuthorizedRole(ulong roleId) => _authorizedRoles.Remove(roleId);
        public bool IsAuthorized(DiscordMember member)
        {
            if (allowAdmins)
            {
                var perms = member.Permissions;
                if (perms.HasPermission(DiscordPermission.Administrator))
                    return true;
            }

            foreach (var role in member.Roles)
            {
                if (_authorizedRoles.Contains(role.Id))
                    return true;
            }

            return false;
        }

        public Task<bool> IsAuthorizedAsync(DiscordMember member)
        {
            return Task.FromResult(IsAuthorized(member));
        }
    }
}
