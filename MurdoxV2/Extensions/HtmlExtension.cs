using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MurdoxV2.Extensions
{
    public static partial class HtmlExtension
    {
        public static string Sanitize(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            string noTags = HtmlTagRegex().Replace(input, string.Empty);
            string decoded = WebUtility.HtmlDecode(noTags);

            return MultiWhitespaceRegex().Replace(decoded, " ").Trim();
        }

        [GeneratedRegex(@"\s{2,}")]
        private static partial Regex MultiWhitespaceRegex();
        [GeneratedRegex("<.*?>")]
        private static partial Regex HtmlTagRegex();
    }
}
