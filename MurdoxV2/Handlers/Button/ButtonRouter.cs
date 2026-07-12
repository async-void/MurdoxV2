namespace MurdoxV2.Handlers.Button;

public sealed class ButtonRouter(IEnumerable<IButtonHandler> handlers)
{
    private readonly IEnumerable<IButtonHandler> _handlers = handlers;

    public bool TryGetHandler(string customId, out IButtonHandler handler)
    {
        handler = _handlers.FirstOrDefault(h => h.CanHandle(customId))!;
        return handler is not null;
    }
}
