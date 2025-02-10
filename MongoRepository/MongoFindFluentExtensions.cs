using System.Diagnostics.CodeAnalysis;
using MongoDB.Driver;

namespace MongoRepository
{
    [ExcludeFromCodeCoverage]
    public static class MongoFindFluentExtensions
    {

        public static IFindFluent<TEntity, TEntity> SelectPage<TEntity>(this IFindFluent<TEntity, TEntity> mongoCollection, int? page, int? pageSize)
        {
            var result = mongoCollection;
            if (page.HasValue && pageSize.HasValue)
            {
                page = page < 1 ? 1 : page;
                pageSize = page < 1 ? 1 : pageSize;
                result = mongoCollection.Skip((page.Value - 1) * pageSize.Value)
                    .Limit(pageSize.Value);
            }
            return result;
        }

    }
}
