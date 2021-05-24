using System;

namespace CartaWeb.Models.DocumentItem
{
    /// <summary>
    /// Represents information about data set storage
    /// </summary>
    public class DatasetItem
    {
        /// <summary>
        /// The data source
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// The data resource
        /// </summary>
        public string Resource { get; set; }
        /// <summary>
        /// Timestamp of when the data set was added to a worksapce
        /// </summary>
        public DateTime? DateAdded { get; set; }
        /// <summary>
        /// The user that added the data set to a workspace
        /// </summary>
        public string AddedBy { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="DatasetItem"/> class
        /// </summary>
        public DatasetItem(string source, string resource, string addedBy)
        {
            Source = source;
            Resource = resource;
            AddedBy = addedBy;
            DateAdded = DateTime.Now;
        }
    }
}
