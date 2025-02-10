using System;
using System.Diagnostics.CodeAnalysis;

namespace MongoRepository
{
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Property)]
    public class EntityGeoIndexAttribute : Attribute
    {
        public string? Name { get; }

        public EntityGeoIndexAttribute()
        {
        }

        public EntityGeoIndexAttribute(string name)
        {
            Name = name;
        }
    }
}
