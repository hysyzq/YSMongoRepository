using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MongoRepository
{
    /// <summary>	A mongo read/write repository. </summary>
    /// <typeparam name="TEntity">	Type of the entity. </typeparam>
    /// <typeparam name="TKey">   	Type of the key. </typeparam>
    [ExcludeFromCodeCoverage]
    public abstract class ReadWriteRepository<TEntity, TKey> : ReadOnlyRepository<TEntity, TKey>,
		IReadWriteRepository<TEntity, TKey>
		where TEntity : class, IEntity<TKey>, new()
	{
        /// <summary>   Constructor. </summary>
        /// <param name="mongoOptions">   The mongoDB connection options. </param>
        /// <param name="factory"> The mongo client factory</param>
        /// <param name="tenantIdentificationService"> tenant Id service (optional) </param>
        /// <param name="customizedIndexService">provide customized index builder (optional) </param>
        protected ReadWriteRepository(
            IOptions<MongoDbOptions> mongoOptions, 
            IMongoClientFactory factory, 
            ITenantIdentificationService? tenantIdentificationService = null, 
            ICustomizedIndexBuilder? customizedIndexService = null) 
            : base(mongoOptions, factory, tenantIdentificationService, customizedIndexService)
		{
            var context = new MongoContext<TEntity>(mongoOptions, factory, tenantIdentificationService);
            Collection = context.Collection(false);
        }

        /// <summary>   Gets the mongoDB collection. </summary>
        /// <value> The mongoDB collection. </value>
        public override IMongoCollection<TEntity>? Collection { get; }


        /// <summary>	Avoids leading or trailing whitespaces in string values. </summary>
        /// <param name="entity">	The entity to trim. </param>
        /// <returns>	A TEntity. </returns>
        protected static TEntity TrimStrings(TEntity entity)
        {
            foreach (var property in typeof(TEntity).GetProperties())
            {
                if (property.PropertyType == typeof(string))
                {
                    var value = (string?)property.GetValue(entity);
                    if (!string.IsNullOrWhiteSpace(value))
                        property.SetValue(entity, value.Trim());
                }
            }
            return entity;
        }

        /// <summary>	Adds entity asynchronously. </summary>
        /// <param name="entity">	The entity to add. </param>
        /// <returns>	A TEntity. </returns>
        public virtual async Task<TEntity> Add(TEntity entity)
        {
            entity = TrimStrings(entity);
            await Collection!.InsertOneAsync(entity).ConfigureAwait(false);
            return entity;
        }

        /// <summary>	Adds a range asynchronously. </summary>
        /// <param name="entities">	An IEnumerable&lt;TEntity&gt; of items to append to this. </param>
        public virtual async Task AddRange(IList<TEntity> entities)
        {
            foreach (var property in typeof(TEntity).GetProperties())
            {
                if (property.PropertyType == typeof(string))
                {
                    foreach (var entity in entities)
                    {
                        var value = (string?)property.GetValue(entity);
                        if (!string.IsNullOrWhiteSpace(value))
                            property.SetValue(entity, value.Trim());
                    }
                }
            }

            await Collection!.InsertManyAsync(entities).ConfigureAwait(false);
        }

        /// <summary>	Updates the given entity asynchronously. </summary>
        /// <param name="entity">	The entity. </param>
        /// <returns>	A TEntity. </returns>
        public virtual async Task<TEntity> Update(TEntity entity)
        {
            entity = TrimStrings(entity);

            var filter = Builders<TEntity>.Filter.Eq(nameof(IEntity<TKey>.Id), entity.Id);
            await Collection!.ReplaceOneAsync(
                 filter,
                 entity,
                 new ReplaceOptions { IsUpsert = true })
                .ConfigureAwait(false);

            return entity;
        }

        /// <summary>	Deletes the given ID asynchronously. </summary>
        /// <param name="id">	The Identifier to delete. </param>
        public virtual async Task Delete(TKey id)
        {
            var filter = Builders<TEntity>.Filter.Eq(nameof(IEntity<TKey>.Id), id);
            await Collection!.DeleteOneAsync(filter).ConfigureAwait(false);
        }

        /// <summary>	FindAndDeleteDeletes the given ID.</summary>
        /// <param name="id">	The Identifier to delete. </param>
        /// <returns>	A TEntity. </returns>
        public async Task<TEntity?> FindAndDelete(TKey id)
        {
            var filter = Builders<TEntity>.Filter.Eq(nameof(IEntity<TKey>.Id), id);
            return await Collection.FindOneAndDeleteAsync(filter).ConfigureAwait(false) ?? default(TEntity?);
        }


        /// <summary>	Updates the given entity asynchronously. </summary>
        /// <param name="entity">	The entity to add. </param>
        /// <param name="versionFieldName">	The Field name used agaist version </param>
        /// <param name="version">	Update Against version </param>
        /// <param name="replaceOptions">option to set collation, hint, IsUpsert etc</param>
        /// <returns> success or not A TEntity. </returns>
        /// <remarks> if versionFieldName and version not provided, this will force upsert </remarks>
        /// <remarks> if versionFieldName & version provided, only success when version matched</remarks>
        public async Task<(bool, TEntity)> UpdateWithVersion(TEntity entity, string versionFieldName, int? version, ReplaceOptions? replaceOptions)
        {
            entity = TrimStrings(entity);
            bool isUpsert = false;

            List<FilterDefinition<TEntity>> filterList = new List<FilterDefinition<TEntity>>();
            filterList.Add(Builders<TEntity>.Filter.Eq(nameof(IEntity<TKey>.Id), entity.Id));
            if (version != null && version > 0 && !string.IsNullOrWhiteSpace(versionFieldName))
            {
                filterList.Add(Builders<TEntity>.Filter.Eq(versionFieldName, version));
            }
            else
            {
                isUpsert = true;
            }

            var filter = Builders<TEntity>.Filter.And(filterList);

            replaceOptions ??= new ReplaceOptions();
            replaceOptions.IsUpsert = isUpsert;

            var result = await Collection!.ReplaceOneAsync(
                 filter,
                 entity,
                 replaceOptions)
                .ConfigureAwait(false);

            var success = isUpsert || result.ModifiedCount > 0;

            return (success, entity);
        }
        
        /// <summary>
        /// Upsert, Find one record and replace it.
        /// </summary>
        /// <param name="entity"> new entity </param>
        /// <param name="filterDefinition"> filter definition to find record </param>
        /// <param name="options"> FindOneAndReplaceOptions options, optional, default upsert:true, return afterDoc </param>
        /// <returns> Entity </returns>
        public async Task<TEntity> Upsert(TEntity entity, FilterDefinition<TEntity> filterDefinition, FindOneAndReplaceOptions<TEntity, TEntity>? options = null)
        {
            options  ??= new FindOneAndReplaceOptions<TEntity, TEntity>()
            {
                IsUpsert = true, 
                ReturnDocument = ReturnDocument.After,
                Sort = null
            };

            return await Collection!.FindOneAndReplaceAsync(filterDefinition, entity, options).ConfigureAwait(false) ;
        }

        public async Task<TEntity> FindOrCreate(
            FilterDefinition<TEntity> filterDefinition,
            Dictionary<Expression<Func<TEntity, object>>, object> fieldValues,
            FindOneAndUpdateOptions<TEntity>? options = null)
        {
            options ??= new FindOneAndUpdateOptions<TEntity>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };

            var updateDefinitions = fieldValues
                .Select(kv => Builders<TEntity>.Update.SetOnInsert(kv.Key, kv.Value))
                .ToArray();

            var updateDefinition = Builders<TEntity>.Update.Combine(updateDefinitions);

            return await Collection!.FindOneAndUpdateAsync(
                filterDefinition,
                updateDefinition,
                options
            );
        }

        public async Task<TEntity> FindAndUpdate(
            FilterDefinition<TEntity> filterDefinition,
            Dictionary<Expression<Func<TEntity, object>>, object> fieldValues,
            FindOneAndUpdateOptions<TEntity>? options = null)
        {
            options ??= new FindOneAndUpdateOptions<TEntity>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };

            var updateDefinitions = fieldValues
                .Select(kv => Builders<TEntity>.Update.Set(kv.Key, kv.Value))
                .ToArray();

            var updateDefinition = Builders<TEntity>.Update.Combine(updateDefinitions);

            return await Collection!.FindOneAndUpdateAsync(
                filterDefinition,
                updateDefinition,
                options
            );
        }

    }
}
