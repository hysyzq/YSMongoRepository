using System.Threading.Tasks;

namespace MongoRepository
{
    public interface IReadWriteWithAuditRepository<TEntity, TKey, TAudit> : IReadWriteRepository<TEntity, TKey>
        where TEntity : class, IEntity<TKey>, new()
        where TAudit : class, IEntity<string>, IAudit, new()
    {
        /// <summary>	Adds entity asynchronously. </summary>
        /// <param name="entity">	The entity to add. </param>
        /// <param name="audit"> the audit entity  ( Optional: if not provided, will use auditDescription to create new one)</param>
        /// <param name="auditDescription"> the audit description ( Optional )</param>
        /// <returns>	A TEntity. </returns>
        Task<TEntity> AddWithAudit(TEntity entity, TAudit audit = default(TAudit), string auditDescription = null);


        /// <summary>	Updates the given entity asynchronously. </summary>
        /// <param name="entity">	The entity to update. </param>
        /// <param name="audit"> The audit object ( Optional: if not provided, will use auditDescription to create new one)</param>
        /// <param name="oldEntity"> The old entity, if it is provided, will not fetch again</param>
        /// <param name="auditDescription"> the audit description ( Optional )</param>
        /// <returns>	A TEntity. </returns>
        Task<TEntity> UpdateWithAudit(TEntity entity, TAudit audit = default(TAudit), TEntity oldEntity = default(TEntity), string auditDescription = null);

        /// <summary>	Deletes the given ID. </summary>
        /// <param name="id">	The Identifier to delete. </param>
        /// <param name="audit"> The audit object  ( Optional: if not provided, will use auditDescription to create new one)</param>
        /// <param name="auditDescription"> the audit description ( Optional )</param>
        Task<TEntity> DeleteWithAudit(TKey id, TAudit audit = default(TAudit), string auditDescription = null);

    }
    
}
