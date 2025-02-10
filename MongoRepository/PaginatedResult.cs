using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MongoRepository
{
    [ExcludeFromCodeCoverage]
    public class PaginatedResult<T> 
    {
        public PageInfo? PageInfo { get; set; }
        public IList<T> Items { get; set; }

        public PaginatedResult()
        {
            Items = new List<T>();
        }
    }

    public class PageInfo
    {
        public int? PageNumber { get; set; }

        public int? PageSize { get; set; }

        public long? TotalCount { get; set; }

        public string? SortBy { get; set; }

        public bool? Desc { get; set; }
    }
}
