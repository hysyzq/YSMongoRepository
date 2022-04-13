using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace MongoRepository
{
    [ExcludeFromCodeCoverage]
    /// <summary>	A mongoDB context for specified entity. </summary>
    /// <typeparam name="TEntity">	Type of the entity. </typeparam>
    public class MongoContext<TEntity>
	{
        /// <summary>
        /// The MongoDB client factory.
        /// </summary>
        private readonly IMongoClientFactory _factory;
        
        /// <summary>   Gets the entities type name. </summary>
        /// <value> The entities type. </value>
        private readonly string _entityTypeName;

        /// <summary>   Gets the database the entities are stored in. </summary>
        /// <value> The entities type. </value>
        private readonly string _entityDatabaseName;

        /// <summary>   Gets the collection the entities are stored in. </summary>
        /// <value> The entities type. </value>
        private readonly string _entityCollectionName;

        /// <summary> Get the index dictionary of the entity </summary>
        private readonly Dictionary<string, MongoIndexDic> _entityIndexDic = new Dictionary<string, MongoIndexDic>();

        /// <summary> Build Index should run once only </summary>
        private static bool _isIndexBuilt;

        private readonly string _expireAtName;

        /// <summary>   Gets the mongo readonly database interface. </summary>
        /// <value> The mongo readonly database interface. </value>
        protected readonly IMongoDatabase ReadOnlyDatabase;

        /// <summary>   Gets the mongo read/write database interface. </summary>
        /// <value> The mongo read/write database interface. </value>
        protected readonly IMongoDatabase ReadWriteDatabase;


        /// <summary>   Constructor. </summary>
        /// <param name="mongoOptions">   The mongoDB connection options. </param>
        /// <param name="factory"> The MongoDb factory </param>
        public MongoContext(IOptions<MongoDbOptions> mongoOptions, IMongoClientFactory factory)
		{
            _factory = factory;
            _entityTypeName = typeof(TEntity).Name;

			var dbAttribute = (EntityDatabaseAttribute)Attribute.GetCustomAttribute(typeof(TEntity), typeof(EntityDatabaseAttribute));
            if (!string.IsNullOrWhiteSpace(dbAttribute?.Database))
            {
                _entityDatabaseName = dbAttribute?.Database;
            }
            else if (!string.IsNullOrWhiteSpace(mongoOptions.Value.DefaultDatabase))
            {
                _entityDatabaseName = mongoOptions.Value.DefaultDatabase;
            }
            else
            {
                _entityDatabaseName = _entityTypeName;
            }
            

			var collectionAttribute = (EntityCollectionAttribute)Attribute.GetCustomAttribute(typeof(TEntity), typeof(EntityCollectionAttribute));
			_entityCollectionName = collectionAttribute?.Collection ?? _entityTypeName;

            if (!_isIndexBuilt)
            {
                var properties = typeof(TEntity).GetProperties();
                foreach (var property in properties)
                {
                    var indexAttr = (EntityIndexAttribute)property.GetCustomAttribute(typeof(EntityIndexAttribute));
                    if (indexAttr != null)
                    {
                        string name = indexAttr.Name??property.Name;
                        if (!_entityIndexDic.ContainsKey(name))
                        {
                            var dic = new MongoIndexDic()
                            {
                                Unique = indexAttr.Unique,
                                Items = new List<string>{ property.Name},
                                Name = indexAttr.Name,
                                CaseInsensitive = indexAttr.CaseInsensitive
                            };
                            _entityIndexDic.Add(name, dic);
                        }
                        else
                        {
                            var dic = _entityIndexDic[name];
                            dic.Items.Add(property.Name);
                        }
                    }

                    var expireAttr = (ExpireIndexAttribute)property.GetCustomAttribute(typeof(ExpireIndexAttribute));
                    if (expireAttr != null)
                    {
                        _expireAtName = property.Name;
                    }
                }
            }

            var readOnlyClient = _factory.GetClient(mongoOptions.Value.ReadOnlyConnection);
            if (readOnlyClient != null)
                ReadOnlyDatabase = readOnlyClient.GetDatabase(_entityDatabaseName);
            var readWriteClient = _factory.GetClient(mongoOptions.Value.ReadWriteConnection);
            if (readWriteClient != null)
                ReadWriteDatabase = readWriteClient.GetDatabase(_entityDatabaseName);

        }

		public IMongoCollection<TEntity> Collection(bool readOnly)
		{
            var collection = ReadWriteDatabase.GetCollection<TEntity>(_entityCollectionName);
            if (!_isIndexBuilt)
            {
                foreach (var indexDic in _entityIndexDic)
                {
                    List<IndexKeysDefinition<TEntity>> indexList = new List<IndexKeysDefinition<TEntity>>();
                    foreach (var item in indexDic.Value.Items)
                    {
                        indexList.Add(Builders<TEntity>.IndexKeys.Ascending(item));
                    }

                    var finalIndex = Builders<TEntity>.IndexKeys.Combine(indexList);

                    var option = new CreateIndexOptions<TEntity> { Background = true, Name = indexDic.Value.Name };

                    if (indexDic.Value.Unique == EntityIndexUnique.True)
                    {
                        option.Unique = true;
                    }
                    if (indexDic.Value.CaseInsensitive == EntityIndexCaseInsensitive.True)
                    {
                        option.Collation = new Collation(locale: "en", strength: CollationStrength.Secondary);
                    }

                    collection.Indexes.CreateOne(new CreateIndexModel<TEntity>(finalIndex, option));
                }

                if (!string.IsNullOrWhiteSpace(_expireAtName))
                {
                    var expireIndex = Builders<TEntity>.IndexKeys.Ascending(_expireAtName);
                    var option = new CreateIndexOptions<TEntity> {ExpireAfter = TimeSpan.Zero};
                    collection.Indexes.CreateOne(new CreateIndexModel<TEntity>(expireIndex, option));
                }

                _isIndexBuilt = true;
            }
            if (readOnly)
            {
                return ReadOnlyDatabase.GetCollection<TEntity>(_entityCollectionName);
            }
            return collection;
        }
	}

    public class MongoIndexDic
    {
        public string Name { get; set; }
        public EntityIndexUnique Unique { get; set; }
        public EntityIndexCaseInsensitive CaseInsensitive { get; set; }
        public List<string> Items { get; set; }
    }
}
