using System;
using System.Diagnostics.CodeAnalysis;

namespace MongoRepository
{
    /// <summary>	Attribute for entity name. </summary>
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Class)]
    public class EntityCollectionAttribute : Attribute
    {
        /// <summary>	Gets or sets the name. </summary>
        /// <value>	The name of the entity. </value>
        public string? Collection { get; set; }

        /// <summary>	Default constructor. </summary>
        public EntityCollectionAttribute()
        {
        }

        /// <summary>	Constructor. </summary>
        /// <param name="collection">	The collection of the entity. </param>
        public EntityCollectionAttribute(string collection)
        {
            Collection = collection;
        }
    }
}
