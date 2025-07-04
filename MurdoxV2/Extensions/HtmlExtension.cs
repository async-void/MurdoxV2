using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MurdoxV2.Extensions
{
    public static class HtmlExtension
    {
        public static string Sanitize(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            string noTags = Regex.Replace(input, "<.*?>", string.Empty);
            string decoded = WebUtility.HtmlDecode(noTags);

            return Regex.Replace(decoded, @"\s{2,}", " ").Trim();
        }
    }
}
