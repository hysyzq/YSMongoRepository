using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MongoRepository
{
    [ExcludeFromCodeCoverage]
    public class MongoIndexDictionary
    {
        public string? Name { get; set; }
        public EntityIndexUnique Unique { get; set; }
        public EntityIndexCaseInsensitive CaseInsensitive { get; set; }
        public List<string>? Items { get; set; }
        public string? PartialFilter { get; set; }
    }
}
