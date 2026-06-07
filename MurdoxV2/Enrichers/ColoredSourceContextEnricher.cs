using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MurdoxV2.Enrichers
{
    public class ColoredSourceContextEnricher : ILogEventEnricher
    {
        private const string Reset = "\x1b[0m";
        private const string NamespaceColor = "\x1b[38;5;245m";
        private const string ClassColor = "\x1b[38;5;208m";
        private const string NumberColor = "\x1b[96m"; 

        private const int Width = 35;

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            // --- SOURCE CONTEXT COLORING ---
            if (!logEvent.Properties.TryGetValue("SourceContext", out var value))
                return;

            var source = (value as ScalarValue)?.Value?.ToString() ?? "";

            var lastDot = source.LastIndexOf('.');
            string ns = lastDot > 0 ? source[..lastDot] : source;
            string cls = lastDot > 0 ? source[(lastDot + 1)..] : source;

            string full = $"{ns}.{cls}";

            string truncated = full;
            if (full.Length > Width)
            {
                int remaining = Width - cls.Length - 4;
                if (remaining < 0) remaining = 0;

                string nsTrimmed = ns.Length > remaining
                    ? ns[^remaining..]
                    : ns;

                truncated = $"...{nsTrimmed}.{cls}";
            }

            int dotIndex = truncated.LastIndexOf('.');
            string nsPart = truncated[..dotIndex];
            string clsPart = truncated[(dotIndex + 1)..];

            string colored =
                $"{NamespaceColor}{nsPart}{Reset}" +
                $"{NamespaceColor}.{Reset}" +
                $"{ClassColor}{clsPart}{Reset}";

            string padded = truncated.PadRight(Width);
            string finalSource = padded.Replace(truncated, colored);

            logEvent.AddOrUpdateProperty(
                propertyFactory.CreateProperty("ColoredSourceContextPadded", finalSource)
            );

            // --- MESSAGE DIGIT COLORING  ---
            string rendered = logEvent.MessageTemplate.Render(logEvent.Properties);

            string coloredMsg = Regex.Replace(
                rendered,
                @"(\x1B\[[0-9;]*m)|(\b\d+\b)",
                m =>
                {
                    if (m.Value.StartsWith("\x1B["))
                        return m.Value;

                    return $"{NumberColor}{m.Value}{Reset}";
                }
            );
            // --- MESSAGE SOURCE CONTEXT COLORING ---
            coloredMsg = Regex.Replace(
                    coloredMsg,
                    "\"([A-Za-z_][A-Za-z0-9_.]+)\"",
                    m =>
                    {
                        string full = m.Groups[1].Value;

                        int lastDot = full.LastIndexOf('.');
                        if (lastDot < 0)
                            return m.Value;

                        string nsPart = full[..lastDot];
                        string clsPart = full[(lastDot + 1)..];

                        string colored =
                            $"{NamespaceColor}{nsPart}{Reset}" +
                            $"{NamespaceColor}.{Reset}" +
                            $"{ClassColor}{clsPart}{Reset}";

                        return $"\"{colored}\"";
                    }
                );
            // --- CLASS NAME COLORING FOR UNQUOTED TYPES ---
            coloredMsg = Regex.Replace(coloredMsg,
                @"\b([A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z_][A-Za-z0-9_]*)+)\b",
                m =>
                {
                    string full = m.Groups[1].Value;

                    // Split on last dot
                    int lastDot = full.LastIndexOf('.');
                    if (lastDot < 0)
                        return m.Value;

                    string nsPart = full[..lastDot];
                    string clsPart = full[(lastDot + 1)..];

                    string colored =
                        $"{NamespaceColor}{nsPart}{Reset}" +
                        $"{NamespaceColor}.{Reset}" +
                        $"{ClassColor}{clsPart}{Reset}";

                    return colored;
                }
            );

            logEvent.AddOrUpdateProperty(
                propertyFactory.CreateProperty("ColoredMessage", coloredMsg)
            );
        }
    }

}
