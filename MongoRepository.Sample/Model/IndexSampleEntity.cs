using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;

namespace MongoRepository.Sample.Model
{
    [EntityDatabase("SampleDatabase")]
    [EntityCollection("IndexSample")]
    [EntityFieldIndex("NestedExpression_compound", "NestedList.Score")]
    [EntityFieldIndex("NestedExpression_compound", "NestedList.Value")]
    [EntityFieldIndex("NestedExpression_order_num", "NestedList.OrderNumber")]
    public class IndexSampleEntity : IEntity<string>
    {
        [BsonId]
        public string Id { get; set; } = string.Empty;

        [EntityIndex]
        public string? Name { get; set; }

        [EntityIndex("AnyName")]
        public string? Field { get; set; }

        [EntityGeoIndex("GeometrySampleIndex")]
        public GeoJsonPolygon<GeoJson2DGeographicCoordinates>? Geometry { get; set; }


        // Compound Index require index names are the same, here is same name as "CompoundSampleIndex"
        [EntityIndex("CompoundSampleIndex", EntityIndexUnique.True, EntityIndexCaseInsensitive.True)]
        public string? CompoundOne { get; set; }
        [EntityIndex("CompoundSampleIndex", EntityIndexUnique.True, EntityIndexCaseInsensitive.True)]
        public string? CompoundTwo { get; set; }


        // Time To Live (TTL) index
        [ExpireIndex]
        public DateTime? ExpireAt { get; set; }

        // index via expression 
        public List<IndexNested>? NestedList { get; set; }

        // index via autoNested attribute
        public List<IndexAutoNested>? NestedAuto { get; set; }

        public IndexAutoSingle? SingleAuto { get; set; }

        // partial index
        [EntityIndex("PartialIndex", EntityIndexUnique.True, EntityIndexCaseInsensitive.True, partialFilter: "{ 'PartialA': 'OK' }")]
        public string? PartialA {get; set; }

        [EntityIndex("PartialIndex", EntityIndexUnique.True, EntityIndexCaseInsensitive.True, partialFilter: "{ 'PartialA': 'OK' }")]
        public string? PartialB { get; set; }

        public string ToDescription()
        {
            return $"IndexSampleEntity with Id: {Id}";
        }
    }
}
