using DSharpPlus.EventArgs;

namespace MurdoxV2.Handlers.Button;

public sealed class UptimeButtonHandler : IButtonHandler
{
    public bool CanHandle(string customId) =>
        customId.Equals("uptimeBtn");
   

    public Task HandleAsync(ComponentInteractionCreatedEventArgs e)
    {
        throw new NotImplementedException();
    }
}
