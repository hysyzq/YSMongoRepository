using System.Collections.Generic;

namespace MongoRepository
{
    public interface ICustomizedIndexBuilder
    {
        /// <summary>
        /// Build your customized index in your way
        /// </summary>
        /// <returns>The Database and collection you have built in a list</returns>
        List<ICustomizedIndexResult> BuildCustomizedIndex();
    }
}
