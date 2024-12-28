using Task_Tracker_WebApp.Cache.Enums;

namespace Task_Tracker_WebApp.Use_Cases.Cache
{
    public interface IMemoryCacheHandler
    {
        void Set<T>(CachePrefix prefixKey, string suffixKey, T value);
        bool Get<T>(CachePrefix prefixKey, string suffixKey, out T? value);
        void Remove(CachePrefix prefixKey, string suffixKey);

    }
}
