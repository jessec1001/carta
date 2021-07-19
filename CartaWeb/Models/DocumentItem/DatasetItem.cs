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
        /// The unique data set identifier, generated when persisting the data set under a workspace
        /// </summary>
        public string Id { get; set; }
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
        public string Name { get; set; }
        /// <summary>
        /// History of the data set item
        /// </summary>
        public DocumentHistory DocumentHistory { get; set; }
        /// <summary>
        /// Optional workflow identifier to apply by default to the data set
        /// </summary>
        public string WorkflowId { get; set; }
        /// <summary>
        /// Optional workflow version number
        /// </summary>
        public int? VersionNumber { get; set; }


        /// <summary>
        /// Creates a new instance of the <see cref="DatasetItem"/> class
        /// </summary>
        public DatasetItem() {}

        /// <summary>
        /// Creates a new instance of the <see cref="DatasetItem"/> class
        /// </summary>
        public DatasetItem(DataSource source, string resource, UserInformation userInformation)
        {
            Source = source;
            Resource = resource;
            DocumentHistory = new DocumentHistory(userInformation);
        }
    }
}
