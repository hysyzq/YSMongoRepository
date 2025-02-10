using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoRepository.Sample.Model;

namespace MongoRepository.Sample.Repositories
{
    public interface ISampleReadWriteRepository : IReadWriteRepository<SampleEntity, ObjectId>;

    public class SampleRepository : ReadWriteRepository<SampleEntity, ObjectId>, ISampleReadWriteRepository
    {
        public SampleRepository(
            IOptions<SampleOptions> mongoOptions,  // you can use different option here for different database.
            IMongoClientFactory factory, 
            ITenantIdentificationService? tenantIdentificationService = null, 
            ICustomizedIndexBuilder? customizedIndexService = null) 
            : base(mongoOptions, factory, tenantIdentificationService, customizedIndexService)
        {
        }
    }
}
