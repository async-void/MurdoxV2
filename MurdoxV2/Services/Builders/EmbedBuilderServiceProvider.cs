using DSharpPlus.Entities;
using MurdoxV2.Models;

namespace MurdoxV2.Services.Builders
{
    public class EmbedBuilderServiceProvider : IDiscordEmbedBuilder
    {
        public DiscordContainerComponent BuildReminderPage(List<Reminder> reminders,int page,int totalPages)
        {
            var chunk = reminders;
            var memberName = chunk[0].Member?.Nickname ?? "Unknown Member";
            var children = new List<DiscordComponent>
            {
                 
                // Title block
                new DiscordTextDisplayComponent($"⏰ **{memberName} Reminders — Page {page} of {totalPages}**"),
                new DiscordSeparatorComponent(true)
            };

            // Each reminder as its own text block
            foreach (var r in chunk)
            {
               
                children.Add(new DiscordTextDisplayComponent($"### 📝 {r.Title}\n"));
                children.Add(new DiscordTextDisplayComponent($"**Content:** {r.Content}"));
                children.Add(new DiscordTextDisplayComponent($"**When:** {r.CreatedAt:g}\t **Complete:** {r.IsComplete}"));
                children.Add(new DiscordSeparatorComponent(true));
            }

            return new DiscordContainerComponent(children);
        }
    }
}
