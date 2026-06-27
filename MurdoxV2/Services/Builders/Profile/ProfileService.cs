using Microsoft.Extensions.Logging;
using MurdoxV2.Interfaces;

namespace MurdoxV2.Services.Builders.Profile
{
    public sealed class ProfileService(IMemberData memDataService, ILogger<ProfileService> logger): IProfile
    {
        private readonly ILogger<ProfileService> _logger = logger;
        private readonly IMemberData _memDataService = memDataService;

        public async Task BuildMemberProfile(ulong memberId, ulong guildId)
        {
            var member = await _memDataService.GetMemberAsync(guildId, memberId);
        }
    }
}
