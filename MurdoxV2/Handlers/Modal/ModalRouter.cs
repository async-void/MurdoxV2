namespace MurdoxV2.Handlers.Modal;

public sealed class ModalRouter(IEnumerable<IModalHandler> handlers)
{
    private readonly Dictionary<string, IModalHandler> _handlers = handlers.ToDictionary(h => h.CustomId);
    public bool TryGetHandler(string customId, out IModalHandler handler)
     => _handlers.TryGetValue(customId, out handler);
}
