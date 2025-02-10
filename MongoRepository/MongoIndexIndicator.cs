using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace MongoRepository
{
    /// <summary>
    /// MongoIndexIndicator is for indicate the indexes of {collection}@{database} have been Built or not.
    /// By keep tracking this can avoid multiple index creation process to be executed.
    /// This indicator is running as a static level.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class MongoIndexIndicator
    {
        public static ConcurrentDictionary<string, bool> BuiltCollections { get; private set; } = new ();

        /// <summary>
        /// Check if index with given indicatorKey has been built or not
        /// </summary>
        /// <param name="indicatorKey"></param>
        /// <returns> True/False </returns>
        public static bool IsBuilt(string indicatorKey)
        {
            try
            {
                return BuiltCollections.TryGetValue(indicatorKey, out _);
            }
            catch (InvalidOperationException)
            {
                // auto force rebuild indicator.
                BuiltCollections = new ();
                return false;
            }
        }

        /// <summary>
        /// Build index has been triggered for the given indicatorKey's Database/Collection
        /// Add to Hash Set to keep tracking it.
        /// </summary>
        /// <param name="indicatorKey"></param>
        public static void BuildTriggered(string indicatorKey)
        {
            BuiltCollections.TryAdd(indicatorKey, true);
        }

        /// <summary>
        /// Clean up the HashSet. Force the index to rebuild when next lazy loading context.
        /// </summary>
        public static void ForceBuild()
        {
            BuiltCollections = new();
        }
    }
}
