using MongoDB.Bson.Serialization.Attributes;

namespace MongoRepository.Sample.Model
{
    [EntityDatabase("SampleDatabase")]
    [EntityCollection("AuditSample")]
    [EntityAudit("Audit")]
    public class AuditSampleEntity : IEntity<string>
    {
        [BsonId]
        public string Id { get; set; }

        public string? Status { get; set; }
        public string ToDescription()
        {
            return $"Audit Sample ID: [{Id}] with [{Status}]";
        }
    }
}
