using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoRepository.Sample.Model;
using MongoRepository.Sample.Repositories;

namespace MongoRepository.Sample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SampleController : ControllerBase
    {
        private readonly ISampleReadWriteRepository _repository;
        private readonly ILogger<SampleController> _logger;

        public SampleController(
            ISampleReadWriteRepository repository,
            ILogger<SampleController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var entity = await _repository.Add(new SampleEntity()
            {
                Name = "Hello World!",
            });

            return Ok(entity);
        }

        [HttpGet("FindOrCreate")]
        public async Task<IActionResult> FindOrCreate()
        {
            // for the whole minute of HH:mm, once it insert one record, it won't insert new one.
            var key = $"ABC-{DateTime.Now.ToString("yyyy-MM-ddTHH:mm")}"; 
            var detail = $"Detail-{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")}";
            var filter = Builders<SampleEntity>.Filter.Eq(t => t.Name, key);

            var getResult = await _repository.Get(filter);

            if (getResult == null)
            {
                _logger.LogInformation("--> No entity with key: {key}", key);
            }
            else
            {
                _logger.LogInformation("---> Entity found with key: {key}, Detail: {detail}", key,getResult.Detail);
            }

            // if not found, then it will insert the following data into DB
            var fieldValues = new Dictionary<Expression<Func<SampleEntity, object>>, object>
            {
                { t => t.Name, key },
                { t => t.Detail, detail }
            };

            var entity = await _repository.FindOrCreate(filter, fieldValues);

            _logger.LogInformation("----> Entity return with key: {key}, Detail: {detail}", key, entity.Detail);

            return Ok(entity);
        }


        [HttpGet("FindAndUpdate")]
        public async Task<IActionResult> FindAndUpdate()
        {
            // for the whole minute of HH:mm, once it insert one record, it won't insert new one.
            var key = $"ABC-{DateTime.Now.ToString("yyyy-MM-ddTHH:mm")}";
            var detail = $"Detail-{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")}";

            try
            {
                await _repository.Add(new SampleEntity() { Name = key, Detail = detail });
                _logger.LogInformation("-> Inserted entity key:{key}, detail: {detail}", key,detail);
            }
            catch (MongoWriteException e) when (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                _logger.LogInformation("--> Found entity key:{key}, detail: {detail}", key, detail);
            }


            var filter = Builders<SampleEntity>.Filter.Eq(t => t.Name, key);

            // if not found, then it will insert the following data into DB
            var fieldValues = new Dictionary<Expression<Func<SampleEntity, object>>, object>
            {
                { t => t.Detail, detail }
                // you can add more field here
            };

            var entity = await _repository.FindAndUpdate(filter, fieldValues);

            _logger.LogInformation("----> Entity updated with key: {key}, Detail: {detail}", key, entity.Detail);

            return Ok(entity);
        }
    }
}
