using System;
using System.Diagnostics.CodeAnalysis;

namespace MongoRepository
{
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class EntityFieldIndexAttribute : Attribute
    {
        public string? Name {get;}
        public string? FieldExpr { get; }

        public EntityFieldIndexAttribute(string name, string fieldExpression)
        {
            Name = name;
            FieldExpr = fieldExpression;
        }
    }
}
