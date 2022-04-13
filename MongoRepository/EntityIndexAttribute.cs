using System;
using System.Diagnostics.CodeAnalysis;

namespace MongoRepository
{
    /// <summary> Attribute for index name. </summary>
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class EntityIndexAttribute : Attribute
    {
        public string? Name { get; }
        public EntityIndexUnique Unique { get; }
        public EntityIndexCaseInsensitive CaseInsensitive { get; }
        public string? PartialFilter { get; }

        public EntityIndexAttribute()
        {
        }

        public EntityIndexAttribute(EntityIndexUnique unique)
        {
            Unique = unique;
        }

        public EntityIndexAttribute(EntityIndexCaseInsensitive caseInsensitive)
        {
            CaseInsensitive = caseInsensitive;
        }

        public EntityIndexAttribute(string name)
        {
            Name = name;
        }

        public EntityIndexAttribute(string name , EntityIndexUnique unique, EntityIndexCaseInsensitive caseInsensitive)
        {
            Unique = unique;
            Name = name;
            CaseInsensitive = caseInsensitive;
        }

        public EntityIndexAttribute(string name, EntityIndexUnique unique, EntityIndexCaseInsensitive caseInsensitive, string? partialFilter)
        {
            Unique = unique;
            Name = name;
            CaseInsensitive = caseInsensitive;
            PartialFilter = partialFilter;  // Set partial filter expression
        }
    }
}
