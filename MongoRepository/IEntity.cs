namespace MongoRepository
{
	/// <summary>	Interface for an entity. </summary>
	/// <typeparam name="TKey">	Type of the key. </typeparam>
	public interface IEntity<TKey>
	{
		/// <summary>	Gets or sets the identifier. </summary>
		/// <value>	The identifier. </value>
		TKey Id { get; set; }

        string ToDescription();
    }
}
