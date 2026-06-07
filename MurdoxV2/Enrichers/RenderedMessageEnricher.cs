using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurdoxV2.Enrichers
{
    public class RenderedMessageEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var rendered = logEvent.MessageTemplate.Render(logEvent.Properties);
            logEvent.AddOrUpdateProperty(
                propertyFactory.CreateProperty("RenderedMessage", rendered)
            );
        }
    }

}
