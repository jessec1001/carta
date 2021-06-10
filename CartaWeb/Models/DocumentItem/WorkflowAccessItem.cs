using CartaWeb.Models.Data;

namespace CartaWeb.Models.DocumentItem
{
    /// <summary>
    /// Used to store information on workflows that are accessible to a user or workspace
    /// </summary>
    public class WorkflowAccessItem
    {
        /// <summary>
        /// The unique workflow identifier
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The workflow name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The workflow version
        /// </summary>
        public VersionInformation VersionInformation { get; set; }


        /// <summary>
        /// Creates a new instance of the <see cref="WorkflowAccessItem"/> class.
        /// </summary>
        public WorkflowAccessItem(string id, string name, VersionInformation versionInformation)
        {
            Id = id;
            Name = name;
            VersionInformation = versionInformation;
        }
    }
}
