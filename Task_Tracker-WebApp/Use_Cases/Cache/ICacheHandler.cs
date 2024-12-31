using Task_Tracker_WebApp.Cache.Enums;

namespace Task_Tracker_WebApp.Use_Cases.Cache
{
    public interface ICacheHandler
    {
        Task Set<T>(CachePrefix prefixKey, string suffixKey, T value) where T : class;
        Task SetList<T>(CachePrefix prefixKey, string suffixKey, IEnumerable<T> value) where T : class;
        Task<T?> Get<T>(CachePrefix prefixKey, string suffixKey) where T : class;
        Task<IEnumerable<T>?> GetList<T>(CachePrefix prefixKey, string suffixKey) where T : class;
        Task<bool> KeyExists(CachePrefix prefixKey, string suffixKey);
        Task Remove(CachePrefix prefixKey, string suffixKey);
    }
}
