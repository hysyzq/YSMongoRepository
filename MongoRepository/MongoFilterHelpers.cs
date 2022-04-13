using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoRepository
{
    [ExcludeFromCodeCoverage]
    public static class MongoFilterHelpers
    {
        public static void SearchOn<T>(
            FilterDefinitionBuilder<T> builder,
            string search,
            string field,
            List<FilterDefinition<T>> filters,
            RegexOptions regexOptions = RegexOptions.IgnoreCase)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var expr = new BsonRegularExpression(new Regex(search, regexOptions));
                var filter = builder.Regex(field, expr);
                filters.Add(filter);
            }
        }

        public static FilterDefinition<T> SearchOn<T>( this FilterDefinitionBuilder<T> builder, string field, string value, RegexOptions regexOptions = RegexOptions.IgnoreCase)
        {
            var filter = builder.Empty;
            if (!string.IsNullOrWhiteSpace(field) && !string.IsNullOrWhiteSpace(value))
            {
                var expr = new BsonRegularExpression(new Regex(value, regexOptions));
                filter = builder.Regex(field, expr);
               
            }
            return filter;
        }

        public static void FindExact<T>(FilterDefinitionBuilder<T> builder, string? value, string? field, List<FilterDefinition<T>> filters, string? delimiter = null)
        {
            if (!string.IsNullOrWhiteSpace(field) && !string.IsNullOrWhiteSpace(value))
            {
                if (delimiter == null)
                {
                    var filter = builder.Eq(field, value);
                    filters.Add(filter);
                }
                else
                {
                    var values = value.Split(delimiter);
                    var filterList = new List<FilterDefinition<T>>();
                    foreach (var val in values)
                    {
                        filterList.Add(builder.Eq(field, val));
                    }
                    filters.Add(builder.Or(filterList));
                }
            }
        }

        public static FilterDefinition<T> SearchOnId<T>(this FilterDefinitionBuilder<T> builder, int? value) where T: IEntity<int>
        {
            var filter = builder.Empty;
            if (value.HasValue)
            {
                filter = builder.Eq(f => f.Id, value);
            }
            return filter;
        }

        public static FilterDefinition<T> SearchOnId<T>(this FilterDefinitionBuilder<T> builder, string value) where T : IEntity<ObjectId>
        {
            var filter = builder.Empty;
            if (!string.IsNullOrEmpty(value))
            {
                filter = builder.Eq(f => f.Id, ObjectId.Parse(value));
            }
            return filter;
        }

        public static (SortDefinition<T> SortDefinition, string SortBy) By<T>(this SortDefinitionBuilder<T> builder, string? sortBy, bool isDescending = false)
        {
            sortBy = sortBy == null || sortBy == "null" || sortBy == "undefined" ? "id" : sortBy;
            BindingFlags bindingFlags = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public;
            var sortProperty = typeof(T).GetProperty(sortBy, bindingFlags)?.Name ?? sortBy;

            return (isDescending ? builder.Descending(sortProperty) : builder.Ascending(sortProperty), sortProperty);
        }
    }
}
