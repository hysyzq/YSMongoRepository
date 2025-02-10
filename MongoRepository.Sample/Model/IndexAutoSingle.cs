namespace MongoRepository.Sample.Model
{
    public class IndexAutoSingle
    {
        [EntityIndex("AutoSingle_compound", EntityIndexUnique.True, EntityIndexCaseInsensitive.True)]
        public string? AutoName { get; set; }
        [EntityIndex("AutoSingle_compound", EntityIndexUnique.True, EntityIndexCaseInsensitive.True)]
        public string? AutoType { get; set; }

        [EntityIndex("AutoSingle_Price")]
        public int? AutoPrice { get; set; }
    }
}
