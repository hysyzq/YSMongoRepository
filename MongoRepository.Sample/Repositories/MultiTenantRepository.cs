using Microsoft.Extensions.Options;
using MongoRepository.Sample.Model;
using MongoRepository.Sample.Services;

namespace MongoRepository.Sample.Repositories
{
    public interface IMultiTenantRepository : IReadWriteRepository<MultiTenantEntity, string>;

    public class MultiTenantRepository : ReadWriteRepository<MultiTenantEntity, string>, IMultiTenantRepository
    {
        public MultiTenantRepository(
            IOptions<SampleOptions> mongoOptions, // you can pass in different settings
            IMongoClientFactory factory,
            IMultiTenantIdentificationService? tenantIdentificationService,  // base on your need, inherit from the ITenantIdentificationService
            ICustomizedIndexBuilder? customizedIndexService = null) 
            : base(mongoOptions, factory, tenantIdentificationService, customizedIndexService)
        {
        }
    }
}
