using MurdoxV2.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MurdoxV2.Services.UrlServices
{
    public partial class UrlCaptureServiceProvider : IUrlCaptureService
    {
        private static readonly Regex UrlRegex = CaptureRegex();
        public IReadOnlyList<string> Capture(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Array.Empty<string>();

            var matches = UrlRegex.Matches(text);

            if (matches.Count == 0)
                return Array.Empty<string>();

            var list = new List<string>(matches.Count);

            foreach (Match m in matches)
                list.Add(m.Value);

            return list;
        }

        // [GeneratedRegex(@"\b((?:[a-z][a-z0-9+\-.]*://|www\.)[^\s<>()]+|(?:[a-z0-9\-]+\.)+[a-z]{2,}(?:/[^\s<>()]*)?)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
        [GeneratedRegex(@"\b((?:[a-z][a-z0-9+\-.]*://|www\.)[^\s<>()]+|(?:[a-z0-9\-]+\.)+(?:com|net|org|edu|gov|mil|int|io|co|ai|app|dev|me|info|biz|tv|us|uk|ca|au|de|fr|jp|cn|in|br|ru|nl|eu|xyz|online|site|tech|store|cloud|pro|name)\b(?:/[^\s<>()]*)?)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
        private static partial Regex CaptureRegex();
    }
}
