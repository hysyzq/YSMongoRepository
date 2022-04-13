using System;

namespace MongoRepository
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ExpireIndexAttribute : Attribute
    {
    }
}
