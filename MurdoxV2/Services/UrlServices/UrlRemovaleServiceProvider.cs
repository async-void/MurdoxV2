using MurdoxV2.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MurdoxV2.Services.UrlServices
{
    public partial class UrlRemovaleServiceProvider : IUrlRemovaleService
    {
        private static readonly Regex UrlRegex = RemovalRegex();
        public string RemoveUrls(string text, bool redact)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            if (redact)
            {
                string cleaned = UrlRegex.Replace(text, "``[redacted]``");

                return cleaned;
            }
            else
            {
                string cleaned = UrlRegex.Replace(text, "");

                return cleaned;
            }         
        }

        [GeneratedRegex(@"\b((?:[a-z][a-z0-9+\-.]*://|www\.)[^\s<>()]+|(?:[a-z0-9\-]+\.)+[a-z]{2,}(?:/[^\s<>()]*)?)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
        private static partial Regex RemovalRegex();
    }
}
