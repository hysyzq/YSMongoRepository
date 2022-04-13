using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace MongoRepository
{
    [ExcludeFromCodeCoverage]
    public class CachedReadOnlyRepository<TEntity, TKey, TCacheIdentifier> : ReadOnlyRepository<TEntity, TKey>, ICachedReadOnlyRepository<TEntity, TKey, TCacheIdentifier>
        where TEntity : class, IEntity<TKey>, new()
    {
        protected readonly ICache Cache;
        protected readonly string Prefix;
        protected readonly string CacheIdentifier;

        public CachedReadOnlyRepository(
            IOptions<MongoDbOptions> mongoOptions,
            IMongoClientFactory factory,
            ICache cache,
            string prefix,
            string cacheIdentifier
            )
            : base(mongoOptions, factory)
        {
            Cache = cache;
            Prefix = prefix;
            CacheIdentifier = cacheIdentifier;
        }

        public virtual async Task<TEntity> CachedGet(TCacheIdentifier identifier, string prefix = null)
        {           
            var key = string.IsNullOrWhiteSpace(prefix)? $"{Prefix}-{identifier}" : $"{prefix}-{identifier}";
            var filterDefinition = Builders<TEntity>.Filter.Eq(CacheIdentifier, identifier);
            if (Cache.TryGet(key, out TEntity cached))
            {
                return cached;
            }

            var entity = await Get(filterDefinition);
            Cache.Set(key, entity);
            return entity;
        }

        public void Invalidate(TCacheIdentifier identifier, string prefix = null)
        {
            var key = string.IsNullOrWhiteSpace(prefix) ? $"{Prefix}-{identifier}" : $"{prefix}-{identifier}";
            Cache.Invalidate(key);
        }
    }
}
