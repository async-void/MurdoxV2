using DSharpPlus.EventArgs;

namespace MurdoxV2.Handlers.Button;

public sealed class WarnButtonHandler : IButtonHandler
{
    public bool CanHandle(string customId) =>
        customId.Equals("warnBtn");
   

    public Task HandleAsync(ComponentInteractionCreatedEventArgs e)
    {
        throw new NotImplementedException();
    }
}
