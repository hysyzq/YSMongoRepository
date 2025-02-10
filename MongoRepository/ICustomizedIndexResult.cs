namespace MongoRepository
{
    public interface ICustomizedIndexResult
    {
        /// <summary>
        /// The Database Name 
        /// </summary>
        string DatabaseName { get; set; }

        /// <summary>
        /// The Collection Name
        /// </summary>
        string CollectionName { get; set; }
    }
}
