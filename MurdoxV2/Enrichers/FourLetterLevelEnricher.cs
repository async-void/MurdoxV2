using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Enrichers
{
    public class FourLetterLevelEnricher : ILogEventEnricher
    {
        private const string Reset = "\u001b[0m";

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            string code = logEvent.Level switch
            {
                LogEventLevel.Verbose => "[VERB]",
                LogEventLevel.Debug => "[DEBG]",
                LogEventLevel.Information => "[INFO]",
                LogEventLevel.Warning => "[WARN]",
                LogEventLevel.Error => "[ERRR]",
                LogEventLevel.Fatal => "[FATL]",
                _ => logEvent.Level.ToString().ToUpper()
            };

            string color = logEvent.Level switch
            {
                LogEventLevel.Verbose => "\u001b[38;5;245m",        // grey
                LogEventLevel.Debug => "\u001b[38;5;33m",           // blue
                LogEventLevel.Information => "\u001b[38;5;34m",     // green
                LogEventLevel.Warning => "\x1b[33m",                // orange
                LogEventLevel.Error => "\u001b[38;5;196m",          // red
                LogEventLevel.Fatal => "\u001b[48;5;196;38;5;15m",  // white on red
                _ => "\u001b[0m"
            };

            string colored = $"{color}{code}{Reset}";

            logEvent.AddOrUpdateProperty(
                propertyFactory.CreateProperty("ColoredLevel", colored)
            );
        }
    }

}
