using Microsoft.Extensions.Options;
using MongoRepository.Sample.Model;

namespace MongoRepository.Sample.Repositories
{

    public interface IAuditSampleRepository : IReadWriteWithAuditRepository<AuditSampleEntity, string, Audit>
    {
    }

    public class AuditSampleRepository : ReadWriteWithAuditRepository<AuditSampleEntity, string, Audit>, IAuditSampleRepository
    {
        public AuditSampleRepository(
            IOptions<SampleOptions> mongoOptions, 
            IMongoClientFactory factory, 
            ILogger<ReadWriteWithAuditRepository<AuditSampleEntity, string, Audit>> logger, 
            IAuditService auditService, 
            ITenantIdentificationService? tenantIdentificationService = null, 
            ICustomizedIndexBuilder? customizedIndexService = null) 
            : base(mongoOptions, factory, logger, auditService, tenantIdentificationService, customizedIndexService)
        {
        }
    }
}
