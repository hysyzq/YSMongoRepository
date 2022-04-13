using System.Diagnostics.CodeAnalysis;

namespace MongoRepository
{
    [ExcludeFromCodeCoverage]
    public class CacheOptions 
    {
        /// <summary>
        /// eviction only happen when memory under pressure
        /// </summary>
        public int SlidingExpirationSeconds { get; set; } = 300;

        /// <summary>
        /// eviction trigger after this timespan
        /// </summary>
        public int AbsoluteExpirationSeconds { get; set; } = 300;

        public int ExpirationScanFrequencyInMs { get; set; } = 1000;
        
        public long MaxItemCount { get; set; } = 10000;

        /// <summary>
        /// TTLSeconds 0 will not force any eviction
        /// </summary>
        public int TtlSeconds { get; set; } = 0;

        public bool DisableCache { get; set; } = false;
    }
}
