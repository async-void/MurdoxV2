using DSharpPlus.EventArgs;

namespace MurdoxV2.Handlers.Button;

public sealed class BanButtonHandler : IButtonHandler
{
    public bool CanHandle(string customId) =>
        customId.Equals("banBtn");
    

    public Task HandleAsync(ComponentInteractionCreatedEventArgs e)
    {
        throw new NotImplementedException();
    }
}
