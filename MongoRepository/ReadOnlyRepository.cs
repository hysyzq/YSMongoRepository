using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MongoRepository
{
    /// <summary>	A mongoDB read only repository. </summary>
    /// <typeparam name="TEntity">	Type of the entity. </typeparam>
    /// <typeparam name="TKey">   	Type of the key. </typeparam>
    [ExcludeFromCodeCoverage]
    public abstract class ReadOnlyRepository<TEntity, TKey> : IReadOnlyRepository<TEntity, TKey>
        where TEntity : class, IEntity<TKey>, new()
    {
        /// <summary>   Constructor. </summary>
        /// <param name="mongoOptions">   The mongoDB connection options. </param>
        /// <param name="factory"> The mongo client factory</param>
        protected ReadOnlyRepository(IOptions<MongoDbOptions> mongoOptions, IMongoClientFactory factory)
        {
            var context = new MongoContext<TEntity>(mongoOptions, factory);
            Collection = context.Collection(true);
        }

        /// <summary>   Gets the mongoDB collection. </summary>
        /// <value> The mongoDB collection. </value>
        public virtual IMongoCollection<TEntity> Collection { get; }


        /// <summary>	Gets a t entity using the given identifier asynchronously. </summary>
        /// <param name="id">	The Identifier to get. </param>
        /// <returns>	A TEntity. </returns>
        public virtual async Task<TEntity> Get(TKey id)
        {
            var filter = Builders<TEntity>.Filter.Eq(nameof(IEntity<TKey>.Id), id);
            TEntity result = await Collection.Find(filter).FirstOrDefaultAsync().ConfigureAwait(false);
            return result;
        }

        /// <summary>	Gets a t entity using the given identifier asynchronously. </summary>
        /// <param name="ids">	The Identifier to get. </param>
        /// <returns>	A TEntity. </returns>
        public virtual async Task<IList<TEntity>> Get(IEnumerable<TKey> ids)
        {
            var filter = Builders<TEntity>.Filter.In(nameof(IEntity<TKey>.Id), ids);
            IList<TEntity> result = await Collection.Find(filter).ToListAsync().ConfigureAwait(false);
            return result;
        }

        /// <summary>	Gets first item in this collection matching a given filter asynchronously. </summary>
        /// <param name="filterDefinition">	A definition to filter the results. Defaults to an empty filter.</param>
        /// <returns>	A TEntity. </returns>
        public virtual async Task<TEntity> Get(FilterDefinition<TEntity> filterDefinition = null)
        {
            TEntity result = await Collection
                .Find(filterDefinition ?? new BsonDocument())
                .FirstOrDefaultAsync().ConfigureAwait(false);
            return result;
        }

        /// <summary>	Gets first item in this collection matching a given filter asynchronously. </summary>
        /// <param name="filter">	A linq expression to filter the results. </param>
        /// <returns>	A TEntity. </returns>
        public virtual async Task<TEntity> Get<TProperty>(Expression<Func<TEntity, bool>> filter)
        {
            TEntity result = await Collection
                .AsQueryable()
                .Where(filter)
                .FirstOrDefaultAsync().ConfigureAwait(false);

            return result;
        }


        /// <summary>	Gets all items in this collection asynchronously. </summary>
        /// <param name="filterDefinition">	A definition to filter the results. Defaults to an empty filter.</param>
        /// <returns>
        ///     An list that allows foreach to be used to process all items in this collection.
        /// </returns>
        public virtual async Task<IList<TEntity>> GetAll(FilterDefinition<TEntity> filterDefinition)
        {
            IList<TEntity> result = await Collection
                .Find(filterDefinition ?? new BsonDocument())
                .ToListAsync().ConfigureAwait(false);
            return result;
        }

        /// <summary>	Gets all items in this collection asynchronously. </summary>
        /// <param name="filterDefinition">	A definition to filter the results. Defaults to an empty filter.</param>
        /// <param name="sortDefinition">	The sorting definition for the result. Defaults to sort ascending by Id.</param>
        /// <returns>
        ///     An list that allows foreach to be used to process all items in this collection.
        /// </returns>
        public virtual async Task<IList<TEntity>> GetAll(FilterDefinition<TEntity> filterDefinition, SortDefinition<TEntity> sortDefinition)
        {
            IList<TEntity> result = await Collection
                .Find(filterDefinition ?? new BsonDocument())
                .Sort(sortDefinition ?? Builders<TEntity>.Sort.Ascending(nameof(IEntity<TKey>.Id)))
                .ToListAsync().ConfigureAwait(false);
            return result;
        }

        /// <summary>	Gets all items in this collection asynchronously. </summary>
        /// <param name="filterDefinition">	A definition to filter the results. Defaults to an empty filter.</param>
        /// <param name="sortDefinition">	The sorting definition for the result. Defaults to sort ascending by Id.</param>
        /// <param name="page">	The requested page number. </param>
        /// <param name="pageSize">	The number of items per page.</param>
        /// <returns>
        ///     An list that allows foreach to be used to process all items in this collection.
        /// </returns>
        public virtual async Task<IList<TEntity>> GetAll(FilterDefinition<TEntity> filterDefinition = null, SortDefinition<TEntity> sortDefinition = null, int? page = null, int? pageSize = null)
        {
            IList<TEntity> result;
            result = await Collection
                .Find(filterDefinition ?? new BsonDocument())
                .SelectPage(page, pageSize)
                .Sort(sortDefinition ?? Builders<TEntity>.Sort.Ascending(nameof(IEntity<TKey>.Id)))
                .ToListAsync().ConfigureAwait(false);
            
            return result;
        }

        /// <summary> Gets paginated result. </summary>
        /// <param name="filterDefinition"> A linq expression to filter the results. </param>       
        /// <param name="page">	The requested page number. </param>
        /// <param name="pageSize">	The number of items per page.</param>
        /// <param name="sortBy">	SortBy property. </param>
        /// <param name="isDescending">	IsDescending sort order. </param>
        /// <returns>
        ///     An list that allows foreach to be used to process all items in this collection.
        /// </returns>
        public virtual async Task<PaginatedResult<TEntity>> GetPaginatedResult(FilterDefinition<TEntity> filterDefinition = null, int? page = null, int? pageSize = null, string sortBy = null, bool isDescending = false)
        {
            var sortByDetails = Builders<TEntity>.Sort.By(sortBy, isDescending);
            IList<TEntity> items = await GetAll(filterDefinition, sortByDetails.SortDefinition, page, pageSize);
            
            var response = new PaginatedResult<TEntity>()
            {
                PageInfo = new PageInfo()
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalCount = await Count(filterDefinition),
                    SortBy = sortByDetails.SortBy,
                    Desc = isDescending
                },
                Items = items
            };

            return response;
        }


        /// <summary>	Gets all items in this collection asynchronously. </summary>
        /// <param name="jsonFilterDefinition">	A definition to filter in a json string the results. Defaults to an empty filter.</param>
        /// <returns>
        ///     An list that allows foreach to be used to process all items in this collection.
        /// </returns>
        public virtual async Task<IList<TEntity>> GetAll(string jsonFilterDefinition)
        {
            JsonFilterDefinition<TEntity> filter = null;
            if (!string.IsNullOrEmpty(jsonFilterDefinition))
            {
                filter = new JsonFilterDefinition<TEntity>(jsonFilterDefinition);
            }

            return await GetAll(filterDefinition: filter);
        }

        /// <summary>	Gets all items in this collection asynchronously. </summary>
        /// <param name="jsonFilterDefinition">	A definition to filter in a json string the results. Defaults to an empty filter.</param>
        /// <param name="jsonSortingDefinition">	The sorting definition in a json string for the result. Defaults to sort ascending by Id.</param>
        /// <returns>
        ///     An list that allows foreach to be used to process all items in this collection.
        /// </returns>
        public virtual async Task<IList<TEntity>> GetAll(string jsonFilterDefinition, string jsonSortingDefinition)
        {
            JsonFilterDefinition<TEntity> filter = null;
            if (!string.IsNullOrEmpty(jsonFilterDefinition))
            {
                filter = new JsonFilterDefinition<TEntity>(jsonFilterDefinition);
            }

            JsonSortDefinition<TEntity> sorting = null;
            if (!string.IsNullOrEmpty(jsonSortingDefinition))
            {
                sorting = new JsonSortDefinition<TEntity>(jsonSortingDefinition);
            }

            return await GetAll(filterDefinition: filter, sortDefinition: sorting);
        }

        /// <summary>	Gets all items in this collection asynchronously. </summary>
        /// <param name="jsonFilterDefinition">	A definition to filter in a json string the results. Defaults to an empty filter.</param>
        /// <param name="jsonSortingDefinition">	The sorting definition in a json string for the result. Defaults to sort ascending by Id.</param>
        /// <param name="page">	The requested page number. </param>
        /// <param name="pageSize">	The number of items per page.</param>
        /// <returns>
        ///     An list that allows foreach to be used to process all items in this collection.
        /// </returns>
        public virtual async Task<IList<TEntity>> GetAll(string jsonFilterDefinition, string jsonSortingDefinition, int? page = null, int? pageSize = null)
        {
            JsonFilterDefinition<TEntity> filter = null;
            if (!string.IsNullOrEmpty(jsonFilterDefinition))
            {
                filter = new JsonFilterDefinition<TEntity>(jsonFilterDefinition);
            }

            JsonSortDefinition<TEntity> sorting = null;
            if (!string.IsNullOrEmpty(jsonSortingDefinition))
            {
                sorting = new JsonSortDefinition<TEntity>(jsonSortingDefinition);
            }

            return await GetAll(filterDefinition: filter, sortDefinition: sorting, page: page, pageSize: pageSize);
        }


        /// <summary>	Gets all items in this collection asynchronously. </summary>
        /// <param name="filter">	A linq expression to filter the results. </param>
        /// <param name="page">	The requested page number. </param>
        /// <param name="pageSize">	The number of items per page.</param>
        /// <returns>
        ///     An list that allows foreach to be used to process all items in this collection.
        /// </returns>
        public virtual async Task<IList<TEntity>> GetAll<TProperty>(Expression<Func<TEntity, bool>> filter, int? page = null, int? pageSize = null)
        {
            IList<TEntity> result;
            result = await Collection
                .AsQueryable()
                .Where(filter)
                .SelectPage(page, pageSize)
                .ToListAsync().ConfigureAwait(false);
            return result;
        }

        /// <summary>	Gets all items in this collection in asynchronously. </summary>
        /// <param name="sorting">	A linq expression to sort the results.</param>
        /// <param name="page">	The requested page number. </param>
        /// <param name="pageSize">	The number of items per page.</param>
        /// <returns>
        ///     An list that allows foreach to be used to process all items in this collection.
        /// </returns>
        public virtual async Task<IList<TEntity>> GetAll<TProperty>(Expression<Func<TEntity, TProperty>> sorting, int? page = null, int? pageSize = null)
        {
            IList<TEntity> result;
            result = await Collection
                .AsQueryable()
                .OrderBy(sorting)
                .SelectPage(page, pageSize)
                .ToListAsync().ConfigureAwait(false);
            return result;
        }

        /// <summary>	Gets all items in this collection asynchronously. </summary>
        /// <param name="filter">	A linq expression to filter the results. </param>
        /// <param name="sorting">	A linq expression to sort the results.</param>
        /// <param name="page">	The requested page number. </param>
        /// <param name="pageSize">	The number of items per page.</param>
        /// <returns>
        ///     An list that allows foreach to be used to process all items in this collection.
        /// </returns>
        public virtual async Task<IList<TEntity>> GetAll<TProperty>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TProperty>> sorting, int? page = null, int? pageSize = null)
        {
            IList<TEntity> result;
            result = await Collection
                .AsQueryable()
                .Where(filter)
                .OrderBy(sorting)
                .SelectPage(page, pageSize)
                .ToListAsync().ConfigureAwait(false);
            return result;
        }


        /// <summary>	Gets all items in this collection in descending order asynchronously. </summary>
        /// <param name="filter">	A linq expression to filter the results. </param>
        /// <param name="page">	The requested page number. </param>
        /// <param name="pageSize">	The number of items per page.</param>
        /// <returns>
        ///     An list that allows foreach to be used to process all items in this collection.
        /// </returns>
        public virtual async Task<IList<TEntity>> GetAllDescending<TProperty>(Expression<Func<TEntity, bool>> filter, int? page = null, int? pageSize = null)
        {
            IList<TEntity> result;
            result = await Collection
                .AsQueryable()
                .Where(filter)
                .SelectPage(page, pageSize)
                .ToListAsync().ConfigureAwait(false);
            return result;
        }

        /// <summary>	Gets all items in this collection in descending order asynchronously. </summary>
        /// <param name="sorting">	A linq expression to sort the results.</param>
        /// <param name="page">	The requested page number. </param>
        /// <param name="pageSize">	The number of items per page.</param>
        /// <returns>
        ///     An list that allows foreach to be used to process all items in this collection.
        /// </returns>
        public virtual async Task<IList<TEntity>> GetAllDescending<TProperty>(Expression<Func<TEntity, TProperty>> sorting, int? page = null, int? pageSize = null)
        {
            IList<TEntity> result;
            result = await Collection
                .AsQueryable()
                .OrderByDescending(sorting)
                .SelectPage(page, pageSize)
                .ToListAsync().ConfigureAwait(false);
            return result;
        }

        /// <summary>	Gets all items in this collection in descending order asynchronously. </summary>
        /// <param name="filter">	A linq expression to filter the results. </param>
        /// <param name="sorting">	A linq expression to sort the results.</param>
        /// <param name="page">	The requested page number. </param>
        /// <param name="pageSize">	The number of items per page.</param>
        /// <returns>
        ///     An list that allows foreach to be used to process all items in this collection.
        /// </returns>
        public virtual async Task<IList<TEntity>> GetAllDescending<TProperty>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TProperty>> sorting, int? page = null, int? pageSize = null)
        {
            IList<TEntity> result;
            result = await Collection
                .AsQueryable()
                .Where(filter)
                .OrderByDescending(sorting)
                .SelectPage(page, pageSize)
                .ToListAsync().ConfigureAwait(false);
            return result;
        }


        /// <summary>
        /// Count total number with provided filter
        /// </summary>
        /// <param name="filterDefinition"></param>
        /// <returns></returns>
        public virtual async Task<long> Count(FilterDefinition<TEntity> filterDefinition = null)
        {
            var result = await Collection
                .Find(filterDefinition ?? new BsonDocument())
                .CountDocumentsAsync().ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Count total number with provided jsonFilterDefinition
        /// </summary>
        /// <param name="jsonFilterDefinition"></param>
        /// <returns></returns>
        public virtual async Task<long> Count(string jsonFilterDefinition)
        {
            JsonFilterDefinition<TEntity> filter = null;
            if (!string.IsNullOrEmpty(jsonFilterDefinition))
            {
                filter = new JsonFilterDefinition<TEntity>(jsonFilterDefinition);
            }
            return await Count(filterDefinition: filter);
        }

        /// <summary>
        /// Count total number with provided expression 
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="filter"></param>
        /// <returns></returns>
        public virtual async Task<long> Count<TProperty>(Expression<Func<TEntity, bool>> filter)
        {
            var result = await Collection
                .AsQueryable()
                .Where(filter)
                .LongCountAsync().ConfigureAwait(false);
            return result;
        }
    }
}
