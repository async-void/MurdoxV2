namespace MurdoxV2.Pagination
{
    public sealed class PaginationRegistry
    {
        private static readonly Dictionary<ulong, object> _contexts = [];

        public static void Register<T>(ulong messageId, PaginationContext<T> ctx) =>
            _contexts[messageId] = ctx;

        public static PaginationContext<T>? Get<T>(ulong messageId) =>
            _contexts.TryGetValue(messageId, out var ctx) && ctx is PaginationContext<T> typedCtx
                ? typedCtx : null;

        public static void Remove(ulong messageId) =>
            _contexts.Remove(messageId);
    }
}
