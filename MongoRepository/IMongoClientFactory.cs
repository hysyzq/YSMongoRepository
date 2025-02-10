using MongoDB.Driver;

namespace MongoRepository
{
    /// <summary>
    /// Constructs mongo clients with application insights telemetry applied
    /// </summary>
    public interface IMongoClientFactory
    {
        /// <summary>
        /// The telemetry settings in use
        /// </summary>
        MongoTelemetrySettings Settings { get; }

        /// <summary>
        /// Get client with MongoClientSettings
        /// </summary>
        /// <param name="clientSettings"></param>
        IMongoClient GetClient(MongoClientSettings clientSettings);

        /// <summary>
        /// Construct a mongo client with MongoUrl
        /// </summary>
        /// <param name="mongoUrl"></param>
        /// <returns>The client</returns>
        IMongoClient GetClient(MongoUrl mongoUrl);

        /// <summary>
        /// Construct a mongo client with telemetry applied
        /// </summary>
        /// <param name="connectionString">The connection string to use</param>
        /// <returns>The client</returns>
        IMongoClient GetClient(string? connectionString);
    }
}
