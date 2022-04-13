using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;

namespace MongoRepository
{
    public interface ICache
    {
        bool TryGet<T>(string key, out T val);

        void Set<T>(string key, T val);

        void Set<T>(string key, T val, MemoryCacheEntryOptions entryOptions);

        void Invalidate(string key);

        void Invalidate(List<string> keys);
    }
}
