using System;
using CartaWeb.Models.Data;

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
        public DataSource Source { get; set; }
        /// <summary>
        /// The data resource
        /// </summary>
        public string Resource { get; set; }
        /// <summary>
        /// Optional alias, to support e.g. an original vs. transformed data set under the same workspace
        /// </summary>
        public string Alias { get; set; }
        /// <summary>
        /// Timestamp of when the data set was added to a worksapce
        /// </summary>
        public DateTime DateAdded { get; set; }
        /// <summary>
        /// The user that added the data set to a workspace
        /// </summary>
        public string AddedBy { get; set; }
        /// <summary>
        /// Optional workflow identifier to apply by default to the data set
        /// </summary>
        public string WorkflowId { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="DatasetItem"/> class
        /// </summary>
        public DatasetItem(DataSource source, string resource, string addedBy)
        {
            Source = source;
            Resource = resource;
            AddedBy = addedBy;
            DateAdded = DateTime.Now;
        }
    }
}
