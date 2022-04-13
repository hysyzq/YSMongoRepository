using System;
using System.Diagnostics.CodeAnalysis;

namespace MongoRepository
{
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Class)]
    public class EntityAuditAttribute : Attribute
    {
        /// <summary>	Gets or sets the name. </summary>
        /// <value>	The name of the audit entity. </value>
        public string AuditCollection { get; set; }

        /// <summary>	Default constructor. </summary>
        public EntityAuditAttribute()
        {
        }

        /// <summary>	Constructor. </summary>
        /// <param name="auditCollection">	The audit collection of the entity. </param>
        public EntityAuditAttribute(string auditCollection)
        {
            AuditCollection = auditCollection;
        }
    }
}
