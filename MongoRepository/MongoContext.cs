using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace MongoRepository
{
    [ExcludeFromCodeCoverage]
    public class MongoContext<TEntity>
	{
        private readonly ITenantIdentificationService? _tenantIdentificationService;

        /// <summary>   Gets the entities type name. </summary>
        /// <value> The entities type. </value>
        private readonly string? _entityTypeName;

        /// <summary>   Gets the database the entities are stored in. </summary>
        /// <value> The entities type. </value>
        private string? _entityDatabaseName;

        /// <summary>   Gets the collection the entities are stored in. </summary>
        /// <value> The entities type. </value>
        private readonly string? _entityCollectionName;

        private readonly string? _indicatorKey;

        /// <summary> Get the index dictionary of the entity </summary>
        private readonly ConcurrentDictionary<string, MongoIndexDictionary> _entityIndexDic = new();

        private readonly ConcurrentDictionary<string, string> _entityGeoIndexDic = new ();
        
        private string? _expireAtName;

        /// <summary>   Gets the mongo readonly database interface. </summary>
        /// <value> The mongo readonly database interface. </value>
        protected readonly IMongoDatabase? ReadOnlyDatabase;

        /// <summary>   Gets the mongo read/write database interface. </summary>
        /// <value> The mongo read/write database interface. </value>
        protected readonly IMongoDatabase? ReadWriteDatabase;


        /// <summary>   Constructor. </summary>
        /// <param name="mongoOptions">   The mongoDB connection options. </param>
        /// <param name="factory"> The MongoDb factory </param>
        /// <param name="tenantIdentificationService">tenant Id service</param>
        /// <param name="customizedIndexService"></param>
        public MongoContext(
            IOptions<MongoDbOptions> mongoOptions, 
            IMongoClientFactory factory, 
            ITenantIdentificationService? tenantIdentificationService = null,
            ICustomizedIndexBuilder? customizedIndexService = null)
		{
            _tenantIdentificationService = tenantIdentificationService;

            _entityTypeName = typeof(TEntity).Name;

			var dbAttribute = (EntityDatabaseAttribute?)Attribute.GetCustomAttribute(typeof(TEntity), typeof(EntityDatabaseAttribute));
            
            SetDatabaseName(mongoOptions, dbAttribute);

			var collectionAttribute = (EntityCollectionAttribute?)Attribute.GetCustomAttribute(typeof(TEntity), typeof(EntityCollectionAttribute));
			_entityCollectionName = collectionAttribute?.Collection ?? _entityTypeName;

            _indicatorKey = $"{_entityCollectionName}@{_entityDatabaseName}";

            if (!MongoIndexIndicator.IsBuilt(_indicatorKey))
            {
                BuildIndexPlan();
                customizedIndexService?.BuildCustomizedIndex();
            }

            if (ReadOnlyDatabase == null)
            {
                var readOnlyClient = factory.GetClient(mongoOptions.Value.ReadOnlyConnection);
                ReadOnlyDatabase = readOnlyClient.GetDatabase(_entityDatabaseName);
            }

            if (ReadWriteDatabase == null)
            {
                var readWriteClient = factory.GetClient(mongoOptions.Value.ReadWriteConnection);
                ReadWriteDatabase = readWriteClient.GetDatabase(_entityDatabaseName);
            }
        }
        
        private void BuildIndexPlan()
        {
            var entityFieldIndexAttributes = (EntityFieldIndexAttribute[]?)Attribute.GetCustomAttributes(typeof(TEntity), typeof(EntityFieldIndexAttribute));

            if (entityFieldIndexAttributes!=null)
            {
                foreach (var fieldIndexAttr in entityFieldIndexAttributes)
                {
                    if (!string.IsNullOrWhiteSpace(fieldIndexAttr.Name) && !string.IsNullOrWhiteSpace(fieldIndexAttr.FieldExpr))
                    {
                        if (_entityIndexDic.TryGetValue(fieldIndexAttr.Name, out var dicItem))
                        {
                            dicItem.Items?.Add(fieldIndexAttr.FieldExpr);
                        }
                        else
                        {
                            _entityIndexDic.TryAdd(fieldIndexAttr.Name, new MongoIndexDictionary()
                            {
                                Name = fieldIndexAttr.Name,
                                Items = new List<string> { fieldIndexAttr.FieldExpr }
                            });
                        }
                    }
                }
            }

            var properties = typeof(TEntity).GetProperties();
            foreach (var property in properties)
            {
                var indexAttrList =
                    ((IEnumerable<EntityIndexAttribute>)property.GetCustomAttributes(typeof(EntityIndexAttribute))).ToList();
                if (indexAttrList.Any())
                {
                    indexAttrList.ForEach(indexAttr =>
                    {
                        string name = indexAttr.Name ?? property.Name;
                        if (!_entityIndexDic.ContainsKey(name))
                        {
                            var dic = new MongoIndexDictionary()
                            {
                                Unique = indexAttr.Unique,
                                Items = new List<string> { property.Name },
                                Name = indexAttr.Name,
                                CaseInsensitive = indexAttr.CaseInsensitive,
                                PartialFilter = indexAttr.PartialFilter
                            };
                            _entityIndexDic.TryAdd(name, dic);
                        }
                        else
                        {
                            var dic = _entityIndexDic[name];
                            dic.Items?.Add(property.Name);
                            if (!string.IsNullOrEmpty(indexAttr.PartialFilter))
                            {
                                dic.PartialFilter = indexAttr.PartialFilter;
                            }
                        }
                    });
                }

                var expireAttr = (ExpireIndexAttribute?)property.GetCustomAttribute(typeof(ExpireIndexAttribute));
                if (expireAttr != null)
                {
                    _expireAtName = property.Name;
                }

                var geoAttr = (EntityGeoIndexAttribute?)property.GetCustomAttribute(typeof(EntityGeoIndexAttribute));
                if (geoAttr != null)
                {
                    _entityGeoIndexDic.TryAdd(geoAttr.Name ?? property.Name, property.Name);
                }

                HandleNestedProperties(property, property.Name);
            }
        }

        private void HandleNestedProperties(PropertyInfo property, string parentName)
        {
            Type? elementType = null;

            // Check if the property is a collection (IEnumerable) and get its element type
            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType) && property.PropertyType.IsGenericType)
            {
                elementType = property.PropertyType.GetGenericArguments().FirstOrDefault();
            }
            else if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
            {
                elementType = property.PropertyType;
            }

            if (elementType == null)
            {
                return;
            }

            var nestedProperties = elementType.GetProperties();
            foreach (var nestedProperty in nestedProperties)
            {
                var nestedIndexAttr = nestedProperty.GetCustomAttribute<EntityIndexAttribute>();
                if (nestedIndexAttr != null)
                {
                    string nestedFieldName = $"{parentName}.{nestedProperty.Name}";
                    string nestedIndexName = nestedIndexAttr.Name ?? nestedFieldName;

                    if (!_entityIndexDic.ContainsKey(nestedIndexName))
                    {
                        var dic = new MongoIndexDictionary()
                        {
                            Unique = nestedIndexAttr.Unique,
                            Items = new List<string> { nestedFieldName },
                            Name = nestedIndexAttr.Name,
                            CaseInsensitive = nestedIndexAttr.CaseInsensitive
                        };
                        _entityIndexDic.TryAdd(nestedIndexName, dic);
                    }
                    else
                    {
                        var dic = _entityIndexDic[nestedIndexName];
                        dic.Items?.Add(nestedFieldName);
                    }
                }
            }
        }

        private void SetDatabaseName(IOptions<MongoDbOptions> mongoOptions, EntityDatabaseAttribute? dbAttribute)
        {
            var suffix = _tenantIdentificationService?.GetCurrentTenantSuffix() ?? string.Empty;
            var prefix = _tenantIdentificationService?.GetCurrentTenantPrefix() ?? string.Empty;


            if (!string.IsNullOrWhiteSpace(dbAttribute?.Database))
            {
                _entityDatabaseName = $"{prefix}{dbAttribute.Database}{suffix}";
            }
            else if (!string.IsNullOrWhiteSpace(mongoOptions.Value.DefaultDatabase))
            {
                _entityDatabaseName = $"{prefix}{mongoOptions.Value.DefaultDatabase}{suffix}";
            }
            else
            {
                _entityDatabaseName = $"{prefix}{_entityTypeName}{suffix}";
            }
        }

        public IMongoCollection<TEntity>? Collection(bool readOnly)
		{
            var collection = ReadWriteDatabase?.GetCollection<TEntity>(_entityCollectionName);
            if (_indicatorKey != null && !MongoIndexIndicator.IsBuilt(_indicatorKey))
            {
                BuildEntityIndex(collection);

                BuildExpireIndex(collection);

                BuildGeoIndex(collection);

                MongoIndexIndicator.BuildTriggered(_indicatorKey);
            }
            if (readOnly)
            {
                return ReadOnlyDatabase?.GetCollection<TEntity>(_entityCollectionName);
            }
            return collection;
        }

        private void BuildGeoIndex(IMongoCollection<TEntity>? collection)
        {
            foreach (var geoIndex in _entityGeoIndexDic)
            {
                var option = new CreateIndexOptions<TEntity> { Background = true, Name = geoIndex.Key };
                var geoIndexKey = Builders<TEntity>.IndexKeys.Geo2DSphere(geoIndex.Value);
                collection?.Indexes.CreateOne(new CreateIndexModel<TEntity>(geoIndexKey, option));
            }
        }

        private void BuildExpireIndex(IMongoCollection<TEntity>? collection)
        {
            if (!string.IsNullOrWhiteSpace(_expireAtName))
            {
                var expireIndex = Builders<TEntity>.IndexKeys.Ascending(_expireAtName);
                var option = new CreateIndexOptions<TEntity> { ExpireAfter = TimeSpan.Zero };
                collection?.Indexes.CreateOne(new CreateIndexModel<TEntity>(expireIndex, option));
            }
        }

        private void BuildEntityIndex(IMongoCollection<TEntity>? collection)
        {
            var existingIndexes = collection?.Indexes.List().ToList().Select(t=>t.GetElement("name").Value.ToString()).ToHashSet();
            
            foreach (var indexDic in _entityIndexDic.Select(t => t.Value))
            {
                if(existingIndexes != null && existingIndexes.Contains(indexDic.Name)) continue;
                
                List<IndexKeysDefinition<TEntity>> indexList = new List<IndexKeysDefinition<TEntity>>();
                foreach (var item in indexDic.Items!.OrderBy(t => t))
                {
                    indexList.Add(Builders<TEntity>.IndexKeys.Ascending(item));
                }
                
                var finalIndex = Builders<TEntity>.IndexKeys.Combine(indexList);

                var option = new CreateIndexOptions<TEntity> { Background = true, Name = indexDic.Name };

                if (indexDic.Unique == EntityIndexUnique.True)
                {
                    option.Unique = true;
                }

                if (indexDic.CaseInsensitive == EntityIndexCaseInsensitive.True)
                {
                    option.Collation = new Collation(locale: "en", strength: CollationStrength.Secondary);
                }

                if (!string.IsNullOrEmpty(indexDic.PartialFilter))
                {
                    var partialFilterBson = BsonDocument.Parse(indexDic.PartialFilter);
                    option.PartialFilterExpression = new BsonDocumentFilterDefinition<TEntity>(partialFilterBson);
                }

                collection?.Indexes.CreateOne(new CreateIndexModel<TEntity>(finalIndex, option));
            }
        }
    }

}
