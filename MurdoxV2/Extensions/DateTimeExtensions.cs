

using MurdoxV2.Enums;

namespace MurdoxV2.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToTimestamp(this DateTimeOffset dto, TimestampFormat format = TimestampFormat.Relative)
            => $"<t:{dto.ToUnixTimeSeconds()}:{(char)format}>";
    }
}
