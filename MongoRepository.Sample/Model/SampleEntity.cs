using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoRepository.Sample.Model
{
    [EntityDatabase("SampleDatabase")]
    [EntityCollection("SampleCollection")]
    public class SampleEntity : IEntity<ObjectId>
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [EntityIndex(EntityIndexUnique.True)]
        public string Name { get; set; }

        public string? Detail { get; set; }

        public string ToDescription()
        {
            return $"This is a sample with {Id}";
        }
    }
}
