using CPUT.Polyglot.NoSql.Interface;
using Microsoft.Extensions.Caching.Memory;
using System.Collections;
using System.Reflection;

namespace CPUT.Polyglot.NoSql.Common
{
    public class Cache : ICache
    {
        private IMemoryCache _cache;

        public Cache(IMemoryCache cache)
        {
            _cache = cache;
        }

        public object GetInMemory(string cacheKey)
        {
            object cacheObject = new object();

            _cache.TryGetValue(cacheKey, out cacheObject);

            return cacheObject;
        }

        public void AddToInMemory(string cacheKey, object cacheObject)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromDays(1));

            _cache.Set(cacheKey, cacheObject, cacheEntryOptions);
        }

        public void AddToInMemoryShortDuration(string cacheKey, object cacheObject)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(30));

            _cache.Set(cacheKey, cacheObject, cacheEntryOptions);
        }

        public void ClearInMemoryWithKey(string cacheKey)
        {
            _cache.Remove(cacheKey);
        }

        public void ClearAllInMemory()
        {
            //Due to there not being a clear method in IMemory cache, 
            //this code is required to get all keys in the cache and remove each key 
            var field = typeof(MemoryCache).GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field.GetValue(_cache) is ICollection collection)
                foreach (var item in collection)
                {
                    var methodInfo = item.GetType().GetProperty("Key");
                    var val = methodInfo.GetValue(item);
                    _cache.Remove(val.ToString());
                }
        }

    }
}
