using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MongoRepository
{
    [ExcludeFromCodeCoverage]
    public class MongoWithAuditContext<TEntity, TAudit> : MongoContext<TEntity>
    {
        /// <summary>   Gets the audit collection the entities are stored in. </summary>
        /// <value> The audit entities type. </value>
        private readonly string _entityAuditCollectionName;

        private readonly string _defaultAuditName = "Audit";

        public MongoWithAuditContext(IOptions<MongoDbOptions> mongoOptions, IMongoClientFactory factory) : base(mongoOptions, factory)
        {
            var auditAttribute = (EntityAuditAttribute)Attribute.GetCustomAttribute(typeof(TEntity), typeof(EntityAuditAttribute));
            _entityAuditCollectionName = auditAttribute?.AuditCollection ?? _defaultAuditName;
        }

        public IMongoCollection<TAudit> AuditCollection()
        {
            var auditCollection = ReadWriteDatabase.GetCollection<TAudit>(_entityAuditCollectionName);
            
            return auditCollection;
        }
    }
}
