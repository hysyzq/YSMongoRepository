using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace MongoRepository
{
    [ExcludeFromCodeCoverage]
    public abstract class ReadWriteWithAuditRepository<TEntity, TKey, TAudit>: ReadWriteRepository<TEntity, TKey>, IReadWriteWithAuditRepository<TEntity, TKey, TAudit>
        where TEntity : class, IEntity<TKey>, new()
        where TAudit : class, IEntity<string>, IAudit, new()
    {
        private readonly ILogger<ReadWriteWithAuditRepository<TEntity, TKey, TAudit>> _logger;

        private readonly IAuditService _auditService;

        /// <summary>   Gets the mongoDB collection. </summary>
        /// <value> The mongoDB collection. </value>
        public override IMongoCollection<TEntity> Collection { get; }

        public IMongoCollection<TAudit> AuditCollection { get; }

        protected ReadWriteWithAuditRepository(IOptions<MongoDbOptions> mongoOptions, IMongoClientFactory factory, ILogger<ReadWriteWithAuditRepository<TEntity, TKey, TAudit>> logger, IAuditService auditService)
            : base(mongoOptions, factory)
        {
            _logger = logger;
            var context = new MongoWithAuditContext<TEntity, TAudit>(mongoOptions, factory);
            Collection = context.Collection(false);
            AuditCollection = context.AuditCollection();
            _auditService = auditService;
        }

        /// <summary>	Adds entity asynchronously. </summary>
        /// <param name="entity">	The entity to add. </param>
        /// <param name="audit"> </param>
        /// <param name="auditDescription"> the audit description </param>
        /// <returns>	A TEntity. </returns>
        /// AuditException will be logged only, will not be thrown 
        public virtual async Task<TEntity> AddWithAudit(TEntity entity, TAudit audit = default(TAudit), string auditDescription = null)
        {
            var result = await base.Add(entity);            
            BuildAuditObject(ref audit, null, result, AuditOperations.Add, auditDescription);
            await AddAudit(audit);
            return result;
        }

        /// <summary>	Updates the given entity asynchronously. </summary>
        /// <param name="entity">	The entity to update. </param>
        /// <param name="audit"> The audit object</param>
        /// <param name="oldEntity"> if old entity is provided, will not fetch again</param>
        /// <param name="auditDescription"> the audit description </param>
        /// <returns>	A TEntity. </returns>
        /// AuditException will be logged only, will not be thrown 
        public async Task<TEntity> UpdateWithAudit(TEntity entity, TAudit audit = default(TAudit), TEntity oldEntity = default(TEntity), string auditDescription = null)
        {
            var old = oldEntity ?? await base.Get(entity.Id);
            var result = await base.Update(entity);
            BuildAuditObject(ref audit, old, result, AuditOperations.Update, auditDescription);
            await AddAudit(audit);
            return result;
        }

        /// <summary> delete entity asynchronously. </summary>
        /// <param name="id"></param>
        /// <param name="audit"> </param>
        /// <param name="auditDescription"> the audit description </param>
        /// AuditException will be logged only, will not be thrown 
        public async Task<TEntity> DeleteWithAudit(TKey id, TAudit audit = default(TAudit), string auditDescription = null)
        {
            var old = await base.FindAndDelete(id);
            if (old == null)
            {
                return null;
            }
            auditDescription ??= old.ToDescription();
            BuildAuditObject(ref audit, old, null, AuditOperations.Delete, auditDescription);
            await AddAudit(audit);
            return old;
        }

        private async Task AddAudit(TAudit audit)
        {
            try
            {
                await AuditCollection.InsertOneAsync(audit).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var auditException = new AuditException(e.Message);
                _logger.LogError(auditException, "Auditing Fail: {object}", JsonConvert.SerializeObject(audit));
            }
        }

        private void BuildAuditObject(ref TAudit audit, TEntity oldItem, TEntity newItem, string auditOperation, string auditDescription)
        {            
            var auditOperationText = string.Format(auditOperation, auditDescription).TrimEnd();
            audit ??= _auditService.GetAuditInformation<TAudit>(auditOperationText);
            audit.Collection = Collection.CollectionNamespace.ToString();
            audit.OldItem = JsonConvert.SerializeObject(oldItem);
            audit.NewItem = JsonConvert.SerializeObject(newItem);
            audit.OperatedAt = DateTimeOffset.UtcNow;
        }
    }
}
