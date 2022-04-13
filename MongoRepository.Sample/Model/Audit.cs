using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace MongoRepository.Sample.Model
{
    [EntityCollection("audit")]
    public class Audit : IEntity<string>, IAudit
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id { get; set; }
        public string ToDescription()
        {
            return $"Audit: Collection: {Collection}, Operation: {Operation}";
        }
        [EntityIndex]
        public string Collection { get; set; }
        public string OldItem { get; set; }
        public string NewItem { get; set; }
        public string Operation { get; set; }
        public string OperatedBy { get; set; }
        [EntityIndex]
        [BsonRepresentation(BsonType.String)]
        public DateTimeOffset OperatedAt { get; set; }
    }
}
