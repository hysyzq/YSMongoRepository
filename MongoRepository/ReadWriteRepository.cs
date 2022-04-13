using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        protected ReadWriteRepository(IOptions<MongoDbOptions> mongoOptions, IMongoClientFactory factory) : base(mongoOptions, factory)
		{
            var context = new MongoContext<TEntity>(mongoOptions, factory);
            Collection = context.Collection(false);
        }

        /// <summary>   Gets the mongoDB collection. </summary>
        /// <value> The mongoDB collection. </value>
        public override IMongoCollection<TEntity> Collection { get; }


        /// <summary>	Avoids leading or trailing whitespaces in string values. </summary>
        /// <param name="entity">	The entity to trim. </param>
        /// <returns>	A TEntity. </returns>
        protected static TEntity TrimStrings(TEntity entity)
        {
            foreach (var property in typeof(TEntity).GetProperties())
            {
                if (property.PropertyType == typeof(string))
                {
                    var value = (string)property.GetValue(entity);
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
            await Collection.InsertOneAsync(entity).ConfigureAwait(false);
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
                        var value = (string)property.GetValue(entity);
                        if (!string.IsNullOrWhiteSpace(value))
                            property.SetValue(entity, value.Trim());
                    }
                }
            }

            await Collection.InsertManyAsync(entities).ConfigureAwait(false);
        }

        /// <summary>	Updates the given entity asynchronously. </summary>
        /// <param name="entity">	The entity. </param>
        /// <returns>	A TEntity. </returns>
        public virtual async Task<TEntity> Update(TEntity entity)
        {
            entity = TrimStrings(entity);

            var filter = Builders<TEntity>.Filter.Eq(nameof(IEntity<TKey>.Id), entity.Id);
            await Collection.ReplaceOneAsync(
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
            await Collection.DeleteOneAsync(filter).ConfigureAwait(false);
        }

        /// <summary>	FindAndDeleteDeletes the given ID.</summary>
        /// <param name="id">	The Identifier to delete. </param>
        /// <returns>	A TEntity. </returns>
        public async Task<TEntity> FindAndDelete(TKey id)
        {
            var filter = Builders<TEntity>.Filter.Eq(nameof(IEntity<TKey>.Id), id);
            return await Collection.FindOneAndDeleteAsync(filter).ConfigureAwait(false);
        }
    }
}
