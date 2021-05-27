﻿using System;

namespace CartaWeb.Models.DocumentItem
{
    /// <summary>
    /// Used to store information such as item name, Id, dates of creation/modification
    /// </summary>
    public class WorkspaceItem
    {
        /// <summary>
        /// The unique worskpace Id, generated when persisting the workspace
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The workspace name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Timestamp of when the workspace was created
        /// </summary>
        public DateTime DateCreated { get; set; }
        /// <summary>
        /// Id of the user that created the workspace
        /// </summary>
        public string CreatedBy { get; set; }
        /// <summary>
        /// Flag to indicate if the workspace has been archived
        /// </summary>
        public bool Archived { get; set; }
        /// <summary>
        /// Timestamp of when the workspace was archived
        /// </summary>
        public DateTime? DateArchived { get; set; }
        /// <summary>
        /// Timestamp of when the workspace was unarchived
        /// </summary>
        public DateTime? DateUnarchived { get; set; }


        /// <summary>
        /// Creates a new instance of the <see cref="WorkspaceItem"/> class with a specified controller.
        /// </summary>
        public WorkspaceItem(string name, string createdBy)
        {
            Name = name;
            CreatedBy = createdBy;
            DateCreated = DateTime.Now;
            Archived = false;
        }
    }
}
