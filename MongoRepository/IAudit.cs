using System;

namespace MongoRepository
{
    public interface IAudit
    {
        /// <summary>
        /// The collection name
        /// </summary>
        public string Collection { get; set; }

        /// <summary>
        /// The old item
        /// </summary>
        public string OldItem { get; set; }
        /// <summary>
        /// The new item
        /// </summary>
        public string NewItem { get; set; }

        /// <summary>
        /// The operation. by default using DefaultOperation
        /// Can be overriden with value defined from application concept, which can help to organize/search.
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// The operation is operated by this user
        /// </summary>
        public string OperatedBy { get; set; }

        /// <summary>
        /// The operation is operated at this DateTimeOffset
        /// </summary>
        public DateTimeOffset OperatedAt { get; set; }
    }
}
