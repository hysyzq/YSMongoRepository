using Microsoft.AspNetCore.Mvc;
using MongoRepository.Sample.Model;
using MongoRepository.Sample.Repositories;

namespace MongoRepository.Sample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MultiTenantController : Controller
    {
        private readonly IMultiTenantRepository _repository;

        public MultiTenantController(IMultiTenantRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var entity = await _repository.Add(new MultiTenantEntity()
            {
                Id = $"Id: {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss:sss}"
            });

            return Ok(entity);
        }
    }
}
