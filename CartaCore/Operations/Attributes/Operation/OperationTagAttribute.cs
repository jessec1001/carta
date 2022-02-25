using System;

namespace CartaCore.Operations.Attributes
{
    /// <summary>
    /// An attribute that assigns a tag to an operation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class OperationTagAttribute : Attribute, IOperationDescribingAttribute
    {
        /// <summary>
        /// A tag to apply to the operation description. Should help to describe the classification of the functionality
        /// of the operation.
        /// </summary>
        public string Tag { get; init; }

        /// <summary>
        /// Assigns a tag to the operation. These should be designed to provide additional context clues to the nature
        /// of an operation. 
        /// </summary>
        /// <param name="tag">The tag to assign.</param>
        public OperationTagAttribute(string tag) => Tag = tag;

        /// <inheritdoc />
        public OperationDescription Modify(OperationDescription description)
        {
            // Ensure tags array is defined.
            if (description.Tags is null)
                description.Tags = Array.Empty<string>();

            // Create a new tags array and append the new tag onto the end.
            string[] tags = new string[description.Tags.Length + 1];
            tags[^1] = Tag;

            // Copy over old tags and return modified tags array.
            description.Tags.CopyTo(tags, 0);
            description.Tags = tags;
            return description;
        }
    }
}