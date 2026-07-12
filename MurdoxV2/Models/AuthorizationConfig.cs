namespace MurdoxV2.Models
{
    public sealed class AuthorizationConfig
    {
        public List<ulong> AuthorizedRoleIds { get; init; } = [];
        public bool AllowAdmins { get; init; } = true;
    }
}
