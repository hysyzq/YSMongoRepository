using System.Threading.Tasks;

namespace MongoRepository
{
    public interface ICachedReadOnlyRepository<TEntity, in TKey, in TCacheIdentifier> : IReadOnlyRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>, new()
    {
        Task<TEntity?> CachedGet(TCacheIdentifier identifier, string? prefix = default);

        void Invalidate(TCacheIdentifier identifier, string? prefix = default);
    }
}
