using System;
using CartaWeb.Models.Data;

namespace CartaWeb.Models.DocumentItem
{
    /// <summary>
    /// Represents user information stored at a workspace level
    /// </summary>
    public class UserItem : Item
    {
        /// <summary>
        /// The user information
        /// </summary>
        public UserInformation UserInformation { get; set; }
        /// <summary>
        /// History of the user item
        /// </summary>
        public DocumentHistory DocumentHistory { get; set; }

        /// <summary>
        /// Parameterless constructor required for deserialization
        /// </summary>
        public UserItem() { }

        /// <summary>
        /// Creates a new instance of the <see cref="UserItem"/> class, used to persist a new user item.
        /// </summary>
        /// <param name="partitionKeyId">The partition key identifier.</param>
        /// <param name="userInformation">User information.</param>
        public UserItem (string partitionKeyId, UserInformation userInformation)
        {
            PartitionKeyId = partitionKeyId;
            Id = userInformation.Id;
            UserInformation = userInformation;
            DocumentHistory = new DocumentHistory(userInformation);
        }

        /// <summary>
        /// Constructor, used to create an instance for reading all items stored under the partition key identifier. 
        /// </summary>
        /// <param name="partitionKeyId">The partition key identifier.</param>
        public UserItem(string partitionKeyId) : base(partitionKeyId) { }

        /// <summary>
        /// Constructor, used to create an instance for reading the item stored under the partition and sort key
        /// identifiers. 
        /// </summary>
        /// <param name="partitionKeyId">The partition key identifier.</param>
        /// <param name="id">The document = sort key identifier.</param>
        public UserItem(string partitionKeyId, string id) : base(partitionKeyId, id) { }

        /// <summary>
        /// Codifies the partition key prefix to use for the document.
        /// </summary>
        /// <returns>The partition key prefix.</returns>
        public override string PartitionKeyPrefix { get { return "WORKSPACE#"; } }

        /// <summary>
        /// Codifies the sort key prefix to use for the document.
        /// </summary>
        /// <returns>The sort key prefix.</returns>
        public override string SortKeyPrefix { get { return "USER#"; } }
    }
}
 