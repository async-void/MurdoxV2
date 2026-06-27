namespace MurdoxV2.Features.AutoLearning
{
    public static class AutoLearningCache
    {
        // Key = GUID string used in the button custom_id
        // Value = the data needed to learn the scam image
        private static readonly Dictionary<string, PendingAutoLearn> _pending = new();

        private static readonly object _lock = new();

        public static void Add(string key, PendingAutoLearn value)
        {
            lock (_lock)
            {
                _pending[key] = value;
            }
        }

        public static bool TryGet(string key, out PendingAutoLearn value)
        {
            lock (_lock)
            {
                return _pending.TryGetValue(key, out value);
            }
        }

        public static void Remove(string key)
        {
            lock (_lock)
            {
                _pending.Remove(key);
            }
        }
    }

}
