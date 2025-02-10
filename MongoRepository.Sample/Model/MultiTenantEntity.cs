using MongoDB.Bson.Serialization.Attributes;

namespace MongoRepository.Sample.Model
{
    [EntityDatabase("SampleDatabase")]
    [EntityCollection("MultiTenantSample")]
    public class MultiTenantEntity : IEntity<string> //this example also show you that you can use string as ID
    {
        [BsonId]
        public string Id { get; set; }

        public string ToDescription()
        {
            return $"MultiTenantSample with Id: {Id}";
        }
    }
}
