using System;

namespace CartaWeb.Models.Data
{
    /// <summary>
    /// Stores version information
    /// </summary>
    public class VersionInformation
    {
        /// <summary>
        /// Version number
        /// </summary>
        public int Number { get; set; }
        /// <summary>
        /// Version description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Timestamp of when the version was created
        /// </summary>
        public DateTime DateCreated { get; set; }
        /// <summary>
        /// Id of the user that created the version
        /// </summary>
        public UserInformation CreatedBy { get; set; }
        /// <summary>
        /// Version number the object was created from
        /// </summary>
        public int BaseNumber { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="VersionInformation"/> class.
        /// </summary>
        public VersionInformation() {}

        /// <summary>
        /// Creates a new instance of the <see cref="VersionInformation"/> class.
        /// </summary>
        public VersionInformation(int number, string description, UserInformation createdBy)
        {
            Number = number;
            Description = description;
            CreatedBy = createdBy;
            DateCreated = DateTime.Now;
        }
    }
}
