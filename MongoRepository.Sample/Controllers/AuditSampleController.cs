using Microsoft.AspNetCore.Mvc;
using MongoRepository.Sample.Model;
using MongoRepository.Sample.Repositories;

namespace MongoRepository.Sample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuditSampleController : ControllerBase
    {
        private readonly IAuditSampleRepository _repository;

        public AuditSampleController(
            IAuditSampleRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var id = "The Entity";
            // delete if existing
            await _repository.DeleteWithAudit(id);

            var entity = new AuditSampleEntity()
            {
                Id = id,
                Status = "Created"
            };
            // add 
            await _repository.AddWithAudit(entity, auditDescription: entity.ToDescription());

            entity.Status = "Modified";
            // update
            var result = await _repository.UpdateWithAudit(entity,auditDescription:entity.ToDescription());
            
            return Ok(result);
        }
    }
}
