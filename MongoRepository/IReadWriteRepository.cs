using System.Collections.Generic;
using System.Threading.Tasks;

namespace MongoRepository
{
    /// <summary>	Interface for a data repository. </summary>
    /// <typeparam name="TEntity">	Type of the entity. </typeparam>
    /// <typeparam name="TKey">   	Type of the key. </typeparam>
    public interface IReadWriteRepository<TEntity, TKey> : IReadOnlyRepository<TEntity, TKey>
        where TEntity : class, IEntity<TKey>, new()
    {
        /// <summary>	Adds entity asynchronously. </summary>
        /// <param name="entity">	The entity to add. </param>
        /// <returns>	A TEntity. </returns>
        Task<TEntity> Add(TEntity entity);

        /// <summary>	Adds a range of entities asynchronously. </summary>
        /// <param name="entities">	An IEnumerable TEntity of items to append to this collection. </param>
        Task AddRange(IList<TEntity> entities);

        /// <summary>	Updates the given entity asynchronously. </summary>
        /// <param name="entity">	The entity to add. </param>
        /// <returns>	A TEntity. </returns>
        Task<TEntity> Update(TEntity entity);

        /// <summary>	Deletes the given ID. </summary>
        /// <param name="id">	The Identifier to delete. </param>
        Task Delete(TKey id);

        /// <summary>	FindAndDeleteDeletes the given ID.</summary>
        /// <param name="id">	The Identifier to delete. </param>
        /// <returns>	A TEntity. </returns>
        Task<TEntity> FindAndDelete(TKey id);
    }
}
