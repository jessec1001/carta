using System;
namespace CartaWeb.Models.Data
{
    /// <summary>
    /// Stores information regarding the add, delete and archive history of a document item
    /// </summary>
    public class DocumentHistory
    {
        /// <summary>
        /// Information of user that added a document item
        /// </summary>
        public UserInformation AddedBy { get; set; }
        /// <summary>
        /// Information of user that deleted a document item
        /// </summary>
        public UserInformation DeletedBy { get; set; }
        /// <summary>
        /// Information of the user that archived the document item
        /// </summary>
        public UserInformation ArchivedBy { get; set; }
        /// <summary>
        /// Information of the user that unarchived the document item
        /// </summary>
        public UserInformation UnarchivedBy { get; set; }
        /// <summary>
        /// Timestamp of the date the document item was added
        /// </summary>
        public DateTime DateAdded { get; set; }
        /// <summary>
        /// Timestamp of the date the document item was deleted
        /// </summary>
        public DateTime? DateDeleted { get; set; }
        /// <summary>
        /// Timestamp of when the document item was archived
        /// </summary>
        public DateTime? DateArchived { get; set; }
        /// <summary>
        /// Timestamp of when the document item was unarchived
        /// </summary>
        public DateTime? DateUnarchived { get; set; }


        /// <summary>
        /// Creates a new instance of the <see cref="DocumentHistory"/> class.
        /// </summary>
        public DocumentHistory() {}

        /// <summary>
        /// Creates a new instance of the <see cref="DocumentHistory"/> class.
        /// </summary>
        public DocumentHistory(UserInformation addedBy)
        {
            AddedBy = addedBy;
            DateAdded = DateTime.Now;
        }

    }
}
