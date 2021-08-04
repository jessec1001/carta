using System;

using CartaCore.Workflow;
using CartaWeb.Models.Data;

namespace CartaWeb.Models.DocumentItem
{
    /// <summary>
    /// Used to store workflow information
    /// </summary>
    public class WorkflowItem
    {
        /// <summary>
        /// The unique workflow identifier, generated when the workflow was created initially
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The workflow
        /// </summary>
        public Workflow Workflow { get; set; }
        /// <summary>
        /// Version information of the workflow
        /// </summary>
        public VersionInformation VersionInformation { get; set; }


        /// <summary>
        /// Creates a new instance of the <see cref="WorkflowItem"/> class.
        /// </summary>
        public WorkflowItem() { }

        /// <summary>
        /// Creates a new instance of the <see cref="WorkflowItem"/> class.
        /// </summary>
        public WorkflowItem(Workflow workflow, VersionInformation versionInformation)
        {
            Workflow = workflow;
            VersionInformation = versionInformation;
        }
    }
}