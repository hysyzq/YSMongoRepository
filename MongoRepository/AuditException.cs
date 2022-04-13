using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace MongoRepository
{
    /// <summary>
    /// Audit exception when audit error occur.
    /// </summary>
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class AuditException : Exception
    {
        public AuditException(string message)
            : base(message)
        {
        }

        protected AuditException(SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
    
}
