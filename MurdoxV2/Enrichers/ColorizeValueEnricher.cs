using MurdoxV2.LogThemes;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Enrichers
{
    public class ColorizeValuesEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            foreach (var prop in logEvent.Properties.ToList())
            {
                string color = logEvent.Level switch
                {
                    LogEventLevel.Information => "\x1b[32m", // green
                    LogEventLevel.Warning => "\x1b[33m",     // yellow
                    LogEventLevel.Error => "\x1b[31m",       // red
                    LogEventLevel.Debug => "\x1b[34m",       // blue
                    LogEventLevel.Fatal => "\x1b[35m",       // magenta
                    _ => "\x1b[0m"
                };
                if (prop.Value is ScalarValue scalar)
                {
                    object? value = scalar.Value;

                    if (value is string s)
                    {
                        string text = $"{color}[{logEvent.Level.ToString()[0..3].ToUpper()}]\x1b[0m";
                        logEvent.AddOrUpdateProperty(
                        propertyFactory.CreateProperty("ColoredLevel", text));

                    }
                    else if (value is int or long or float or double or decimal)
                    {
                        logEvent.AddOrUpdateProperty(
                            propertyFactory.CreateProperty(
                                prop.Key,
                                $"{AnsiTheme.Cyan}{value}{AnsiTheme.Reset}"
                            )
                        );
                    }
                }
            }
        }
    }
}
