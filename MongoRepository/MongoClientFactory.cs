using Microsoft.ApplicationInsights;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MongoRepository
{
    /// <summary>
    /// Constructs mongo clients with application insights telemetry applied
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class MongoClientFactory : IMongoClientFactory
    {
        private readonly TelemetryClient _telemetryClient;

        /// <summary>
        /// Client pool to keep the client singleton as recommended.
        /// MongoClient is thread-safe
        /// ref: https://mongodb.github.io/mongo-csharp-driver/2.9/reference/driver/connecting/#re-use
        /// </summary>
        private readonly Dictionary<string, IMongoClient> _clientPool = new Dictionary<string, IMongoClient>();
        
        /// <summary>
        /// The telemetry settings in use
        /// </summary>
        public MongoApplicationInsightsSettings Settings { get; }

        /// <summary>
        /// Construct a factory
        /// </summary>
        /// <param name="telemetryClient">The telemetry client to send telemetry to</param>
        /// <param name="settings">Telemetry settings</param>
        public MongoClientFactory(
            TelemetryClient telemetryClient = null,
            MongoApplicationInsightsSettings settings = null)
        {
            _telemetryClient = telemetryClient;
            Settings = settings ?? new MongoApplicationInsightsSettings();
        }

        /// <summary>
        /// Construct a mongo client with telemetry applied
        /// </summary>
        /// <param name="clientSettings">The mongo client settings to use</param>
        /// <returns>The client</returns>
        public IMongoClient GetClient(MongoClientSettings clientSettings)
        {
            if (_telemetryClient != null)
            {
                var mongoApplicationInsightsTelemetry = new MongoApplicationInsightsTelemetry();
                mongoApplicationInsightsTelemetry.Setup(clientSettings, _telemetryClient, Settings);
            }
            var key = clientSettings.ToString();
            if (!_clientPool.TryGetValue(key, out var client))
            {
                client = new MongoClient(clientSettings);
                if (_telemetryClient != null) _clientPool.Add(key, client);
            }
            return client;
        }

        /// <summary>
        /// Construct a mongo client with telemetry applied
        /// </summary>
        /// <param name="connectionString">The connection string to use</param>
        /// <returns>The client</returns>
        public IMongoClient GetClient(string connectionString)
        {
            return GetClient(
                MongoClientSettings.FromConnectionString(connectionString));
        }

        /// <summary>
        /// Construct a mongo client with telemetry applied
        /// </summary>
        /// <param name="mongoUrl">The connection string to use</param>
        /// <returns>The client</returns>
        public IMongoClient GetClient(MongoUrl mongoUrl)
        {
            return GetClient(MongoClientSettings.FromUrl(mongoUrl));
        }
    }
}
