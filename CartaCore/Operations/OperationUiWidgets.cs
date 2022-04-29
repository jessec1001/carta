using CartaCore.Operations.Attributes;

namespace CartaCore.Operations
{
    // TODO: Create a UI widget for graphs that allows easy creation/modification. 
    /// <summary>
    /// Contains a set of possible UI widgets that can be used for operation fields. This list should be maintained in
    /// order to be consistent with the schema form generation library. A UI widget may be chosen from the relevant
    /// members of this class (pertaining to a specific type) and used in conjunction with the
    /// <see cref="FieldUiWidgetAttribute" />. 
    /// </summary>
    public static class OperationUiWidgets
    {
        /// <summary>
        /// UI widgets that apply to boolean values.
        /// </summary>
        public static class Booleans
        {
            /// <summary>
            /// Displays a simple checkbox that can be used to toggle a boolean value.
            /// </summary>
            public const string Checkbox = "checkbox";

        }
        /// <summary>
        /// UI widgets that apply to numeric values.
        /// </summary>
        public static class Numbers
        {
            /// <summary>
            /// Displays a simple text box that can be used to enter a numeric value.
            /// </summary>
            public const string Field = "field";
            /// <summary>
            /// Displays a slider that can be used to enter a numeric value from a range.
            /// This widget should only be used in conjunction with the <see cref="FieldRangeAttribute" /> where both
            /// the minimum and maximum values are specified.
            /// </summary>
            public const string Slider = "slider";
        }
        /// <summary>
        /// UI widgets that apply to enumeration values.
        /// </summary>
        public static class Enumerations
        {
            /// <summary>
            /// Displays a dropdown menu that can be used to select an enumeration value.
            /// </summary>
            public const string Dropdown = "dropdown";
        }
        /// <summary>
        /// UI widgets that apply to string values.
        /// </summary>
        public static class Strings
        {
            /// <summary>
            /// Displays a simple text box that can be used to enter a string value.
            /// </summary>
            public const string Field = "field";
            /// <summary>
            /// Displays an adjustable-size text area that can be used to enter a string value.
            /// </summary>
            public const string Area = "area";
            /// <summary>
            /// Displays a user selection text field.
            /// From this text field, a user can be selected via username or email address with autocomplete.
            /// </summary>
            public const string User = "user";
            /// <summary>
            /// Displays a resource selection text field.
            /// From this text field, a resource can be selected via resource name.
            /// Can be combined with a <c>"ui:filter"</c> to filter based on source.
            /// </summary>
            public const string Resource = "resource";
        }
        /// <summary>
        /// UI widgets that apply to array values.
        /// </summary>
        public static class Arrays
        {
            /// <summary>
            /// Displays a list of entries for the array.
            /// </summary>
            public const string List = "list";
        }
        /// <summary>
        /// UI widgets that apply to object values.
        /// </summary>
        public static class Objects
        {
            /// <summary>
            /// Displays a list of properties for the object.
            /// </summary>
            public const string List = "list";
        }
        /// <summary>
        /// UI widgets that apply to multiple-typed values.
        /// </summary>
        public static class Multitypes
        {
            /// <summary>
            /// Displays a dropdown that allows for selecting the type of a value.
            /// Once a type is selected, a corresponding value can be entered.
            /// </summary>
            public const string Dropdown = "dropdown";
            /// <summary>
            /// Displays a checkbox that allows for toggling the nullability of a value.
            /// If the checkbox is unchecked, the value is null.
            /// If the checkbox is checked, the value may be entered by the user as the corresponding type.
            /// This widget should only be used for nullable types.
            /// </summary>
            public const string Checkbox = "checkbox";
        }
        /// <summary>
        /// UI widgets that apply to file values.
        /// </summary>
        public static class Files
        {
            /// <summary>
            /// Displays a drag and drop area that can be used to upload a file.
            /// </summary>
            public const string DragDrop = "dragdrop";
        }
    }
}