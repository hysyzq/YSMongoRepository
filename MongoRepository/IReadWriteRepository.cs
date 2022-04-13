using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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
        Task<TEntity?> FindAndDelete(TKey id);

        /// <summary>	Updates the given entity asynchronously. </summary>
        /// <param name="entity">	The entity to add. </param>
        /// <param name="versionFieldName">	The Field name used agaist version </param>
        /// <param name="version">	Update Against version </param>
        /// <param name="replaceOptions">option to set collation, hint, IsUpsert etc</param>
        /// <returns> success or not A TEntity. </returns>
        /// <remarks> if versionFieldName and version not provided, this will force upsert </remarks>
        /// <remarks> if versionFieldName & version provided, only success when version matched</remarks>
        Task<(bool, TEntity)> UpdateWithVersion(TEntity entity, string versionFieldName, int? version, ReplaceOptions? replaceOptions);

        /// <summary>
        /// Upsert, Find one record and replace it.
        /// </summary>
        /// <param name="entity"> new entity </param>
        /// <param name="filterDefinition"> filter definition to find record </param>
        /// <param name="options"> FindOneAndReplaceOptions options, optional, default upsert:true, return afterDoc </param>
        /// <returns> Entity </returns>
        Task<TEntity> Upsert(TEntity entity, FilterDefinition<TEntity> filterDefinition, FindOneAndReplaceOptions<TEntity, TEntity>? options = null);

        /// <summary>
        /// Finds one document matching the given filter.
        /// If a matching document is found, it is returned unchanged.
        /// If no matching document exists, a new one is created with the provided field values.
        /// </summary>
        /// <param name="filterDefinition">The filter definition used to find the record.</param>
        /// <param name="fieldValues">
        /// A dictionary where:
        /// - Keys are field selectors (`Expression<Func<TEntity, object>>`) representing fields to insert.
        /// - Values are the corresponding values to set when inserting.
        ///
        /// Example:
        /// ```csharp
        /// var fieldValues = new Dictionary<Expression<Func<SampleEntity, object>>, object>
        /// {
        ///     { t => t.Name, "ABC-2025-01-22T12:30" },
        ///     { t => t.Detail, "Initial Detail Information" },
        ///     { t => t.CreatedAt, DateTime.UtcNow }
        /// };
        /// ```
        /// </param>
        /// <param name="options">
        /// `FindOneAndUpdateOptions` (optional). Default: `IsUpsert = true`, `ReturnDocument = ReturnDocument.After`.
        /// </param>
        /// <returns>Returns the found or newly created entity.</returns>
        Task<TEntity> FindOrCreate(FilterDefinition<TEntity> filterDefinition, Dictionary<Expression<Func<TEntity, object>>, object> fieldValues, FindOneAndUpdateOptions<TEntity>? options = null);

        /// <summary>
        /// Finds one document matching the given filter and updates the specified fields.
        /// If a matching document is found, the specified fields are updated.
        /// If no matching document exists, a new one is created with the provided field values (if `IsUpsert` is true).
        /// </summary>
        /// <param name="filterDefinition">The filter definition used to find the record.</param>
        /// <param name="fieldValues">
        /// A dictionary where:
        /// - Keys are field selectors (`Expression<Func<TEntity, object>>`) representing fields to update.
        /// - Values are the corresponding values to set.
        /// 
        /// The provided values will **always** update the existing document.  
        /// If the document does not exist, it will be inserted **only if `IsUpsert` is set to true**.
        /// 
        /// Example:
        /// ```csharp
        /// var fieldValues = new Dictionary<Expression<Func<SampleEntity, object>>, object>
        /// {
        ///     { t => t.Detail, "Updated Detail Information" },
        ///     { t => t.ModifiedAt, DateTime.UtcNow }
        /// };
        /// ```
        /// </param>
        /// <param name="options">
        /// `FindOneAndUpdateOptions` (optional). Default: `IsUpsert = true`, `ReturnDocument = ReturnDocument.After`.
        /// </param>
        /// <returns>Returns the updated or inserted entity.</returns>
        Task<TEntity> FindAndUpdate(FilterDefinition<TEntity> filterDefinition, Dictionary<Expression<Func<TEntity, object>>, object> fieldValues, FindOneAndUpdateOptions<TEntity>? options = null);
    }
}
