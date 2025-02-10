using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using MongoRepository.Sample.Model;
using MongoRepository.Sample.Repositories;

namespace MongoRepository.Sample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IndexSampleController : Controller
    {
        private readonly IIndexSampleRepository _repository;

        public IndexSampleController(IIndexSampleRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var timestamp = $"{DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss:sss}";
            var id = $"Id: {timestamp}";
            var entity = new IndexSampleEntity()
            {
                Id = id,
                Name = "Index-Sample",
                Field = "WithAnyIndexName",
                Geometry = GeoJson.Polygon(new GeoJson2DGeographicCoordinates[]
                {
                    new GeoJson2DGeographicCoordinates(151.195767, -33.853846),
                    new GeoJson2DGeographicCoordinates(151.221394, -33.853747),
                    new GeoJson2DGeographicCoordinates(151.207708, -33.885416),
                    new GeoJson2DGeographicCoordinates(151.195767, -33.853846),
                }),
                CompoundOne = $"One {timestamp}",
                CompoundTwo = $"Two {timestamp}",
                ExpireAt = DateTime.UtcNow.AddMinutes(2),
                NestedList = new List<IndexNested>
                {
                    new IndexNested()
                    {
                        Value = $"123 {timestamp}"
                    }
                },
                NestedAuto = new List<IndexAutoNested>
                {
                    new IndexAutoNested()
                    {
                        AutoName = $"ABC {timestamp}",
                        AutoType = $"XYZ {timestamp}",
                        AutoPrice = 123,
                    }
                },
                SingleAuto = new IndexAutoSingle
                {
                    AutoName = $"ABC {timestamp}",
                    AutoType = $"XYZ {timestamp}",
                    AutoPrice = 123,
                },
                PartialA = "OK",
                PartialB = "KO"
            };
            try
            {
                await _repository.Add(entity);
            }
            catch (MongoWriteException e) when (e.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                entity.PartialA = "DuplicateKey detected for Partial Expression";
                await _repository.Add(entity);
            }
            var result = await _repository.Get(id);

            return Ok(result);
        }
    }
}
