using Microsoft.Extensions.Caching.Memory;
using Task_Tracker_WebApp.Cache.Enums;

namespace Task_Tracker_WebApp.Cache
{
    public class MemoryCacheHandler
    {
        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _options;

        private readonly int slidingExpMins = 30;
        private readonly int absoluteExpHours = 1;

        public MemoryCacheHandler(IMemoryCache cache)
        {
            _cache = cache;
            _options = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(slidingExpMins))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(absoluteExpHours));
        }

        public void Set<T>(CachePrefix prefixKey, string suffixKey, T value)
            => _cache.Set(getPrefix(prefixKey) + suffixKey, value, _options);

        public bool Get<T>(CachePrefix prefixKey, string suffixKey, out T? value)
        {
            if(!_cache.TryGetValue(getPrefix(prefixKey) + suffixKey, out value))
                return false;

            return true;
        }

        public void Remove(CachePrefix prefixKey, string suffixKey)
            => _cache.Remove(getPrefix(prefixKey) + suffixKey);

        private string getPrefix(CachePrefix prefix)
        {
            return prefix switch
            {
                CachePrefix.UserTaskList => "UserTaskList_",
                CachePrefix.UserTask => "UserTask_",
                _ => throw new ArgumentOutOfRangeException(nameof(prefix), prefix, null)
            };
        }
    }
}
