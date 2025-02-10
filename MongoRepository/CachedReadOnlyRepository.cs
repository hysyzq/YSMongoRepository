using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MongoRepository
{
    [ExcludeFromCodeCoverage]
    public class CachedReadOnlyRepository<TEntity, TKey, TCacheIdentifier> : ReadOnlyRepository<TEntity, TKey>, ICachedReadOnlyRepository<TEntity, TKey, TCacheIdentifier>
        where TEntity : class, IEntity<TKey>, new()
    {
        protected readonly ICache Cache;
        protected readonly string Prefix;
        protected readonly string CacheIdentifier;
        private readonly ITenantIdentificationService? _tenantIdentificationService;

        /// <summary>
        /// Read only Cached repository
        /// </summary>
        /// <param name="mongoOptions">The mongoDB connection options.</param>
        /// <param name="factory">The mongo client factory</param>
        /// <param name="cache">The ICache service to implement cache</param>
        /// <param name="prefix">cache prefix</param>
        /// <param name="cacheIdentifier">cache identifier</param>
        /// <param name="tenantIdentificationService"> tenant ID service (optional) </param>
        /// <param name="customizedIndexService">provide customized index builder (optional) </param>
        public CachedReadOnlyRepository(
            IOptions<MongoDbOptions> mongoOptions,
            IMongoClientFactory factory,
            ICache cache,
            string prefix,
            string cacheIdentifier,
            ITenantIdentificationService? tenantIdentificationService = null,
            ICustomizedIndexBuilder? customizedIndexService = null
            )
            : base(mongoOptions, factory, tenantIdentificationService, customizedIndexService)
        {
            Cache = cache;
            Prefix = prefix;
            CacheIdentifier = cacheIdentifier;
            _tenantIdentificationService = tenantIdentificationService;
        }

        public virtual async Task<TEntity?> CachedGet(TCacheIdentifier identifier, string? prefix = default)
        {
            var TenantKey =
                $"{_tenantIdentificationService?.GetCurrentTenantPrefix()}:{_tenantIdentificationService?.GetCurrentTenantSuffix()}";
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

        public void Invalidate(TCacheIdentifier identifier, string? prefix = default)
        {
            var key = string.IsNullOrWhiteSpace(prefix) ? $"{Prefix}-{identifier}" : $"{prefix}-{identifier}";
            Cache.Invalidate(key);
        }
    }
}
