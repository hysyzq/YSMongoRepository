namespace MongoRepository.Sample.Model
{
    public class IndexAutoNested
    {
        [EntityIndex("AutoIndex_compound", EntityIndexUnique.True, EntityIndexCaseInsensitive.True)]
        public string? AutoName { get; set; }
        [EntityIndex("AutoIndex_compound", EntityIndexUnique.True, EntityIndexCaseInsensitive.True)]
        public string? AutoType { get; set; }

        [EntityIndex("AutoIndex_Price")]
        public int? AutoPrice { get; set; }
    }
}
