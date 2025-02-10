using Microsoft.Extensions.Options;
using MongoRepository.Sample.Model;

namespace MongoRepository.Sample.Repositories
{
    public interface IIndexSampleRepository : IReadWriteRepository<IndexSampleEntity, string>;
    public class IndexSampleRepository : ReadWriteRepository<IndexSampleEntity, string>, IIndexSampleRepository
    {
        public IndexSampleRepository
            (IOptions<SampleOptions> mongoOptions, 
                IMongoClientFactory factory, 
                ITenantIdentificationService? tenantIdentificationService = null, 
                ICustomizedIndexBuilder? customizedIndexService = null) 
            : base(mongoOptions, factory, tenantIdentificationService, customizedIndexService)
        {
        }
    }
}
