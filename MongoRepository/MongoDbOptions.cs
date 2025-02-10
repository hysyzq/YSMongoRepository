namespace MongoRepository
{
    /// <summary>	Connection options for a mongoDB context. </summary>
    public class MongoDbOptions
    {
        public string? ReadWriteConnection { get; set; }

        public string? ReadOnlyConnection { get; set; }

        public string? DefaultDatabase { get; set; }

        public bool EnableDebugMode { get; set; } = false; // if true, log the commands
    }
}
