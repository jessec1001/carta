﻿using System;
namespace CartaWeb.Models.Data
{
    /// <summary>
    /// Encapsulates additional information aboute changes made in a workspace
    /// </summary>
    public class WorkspaceChangeInformation
    {

        /// <summary>
        /// The workflow ID assigned to a dataset in a workspace
        /// </summary>
        public string WorkflowId { get; set; }

        /// <summary>
        /// The workflow name 
        /// </summary>
        public string WorkflowName { get; set; }

        /// <summary>
        /// The workflow version 
        /// </summary>
        public int? WorkflowVersion { get; set; }

        /// <summary>
        /// The dataset source 
        /// </summary>
        public string DatasetSource { get; set; }

        /// <summary>
        /// The dataset resource 
        /// </summary>
        public string DatasetResource { get; set; }
    }
}