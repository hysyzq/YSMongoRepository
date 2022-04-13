using System.Diagnostics.CodeAnalysis;
using MongoDB.Driver.Linq;

namespace MongoRepository
{
    [ExcludeFromCodeCoverage]
    public static class MongoQueryableExtensions
    {
        public static IMongoQueryable<TEntity> SelectPage<TEntity>(this IMongoQueryable<TEntity> mongoCollection, int? page, int? pageSize)
        {
            var result = mongoCollection;
            if (page.HasValue && pageSize.HasValue)
            {
                page = page < 1 ? 1 : page;
                pageSize = page < 1 ? 1 : pageSize;
                result = mongoCollection.Skip((page.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value);
            }
            return result;
        }
    }
}
