namespace CartaCore.Operations
{
    /// <summary>
    /// Contains a set of commonly used tags for operations. This list should be consulted first for tags applicable to
    /// an operation to maintain consistency. In order for these tags to be used parsed effectively by a client, they
    /// should not contain whitespace or special characters.
    /// </summary>
    public static class OperationTags
    {
        #region Internal
        /// <summary>
        /// Related to a workflow operation, its structure, and its execution.
        /// </summary>
        public const string Workflow = "Workflow";
        #endregion

        #region Data Algorithms
        /// <summary>
        /// Related to the generation of synthetic data.
        /// </summary>
        public const string Synthetic = "Synthetic";
        /// <summary>
        /// Related to loading data from a data source.
        /// </summary>
        public const string Loading = "Loading";
        /// <summary>
        /// Related to saving data to a data source.
        /// </summary>
        public const string Saving = "Saving";
        /// <summary>
        /// Related to parsing data in a known format.
        /// </summary>
        public const string Parsing = "Parsing";
        /// <summary>
        /// Related to converting between equivalent data formats.
        /// </summary>
        public const string Conversion = "Conversion";
        /// <summary>
        /// Related to visualizing the structure data.
        /// </summary>
        public const string Visualization = "Visualization";
        #endregion

        #region Data Structures
        /// <summary>
        /// Related to text data.
        /// </summary>
        public const string Text = "Text";
        /// <summary>
        /// Related to an array data structure. 
        /// </summary>
        public const string Array = "Array";
        /// <summary>
        /// Related to a graph data structure.
        /// </summary>
        public const string Graph = "Graph";
        #endregion

        #region Integration
        /// <summary>
        /// Related to HyperThought integration.
        /// </summary>
        public const string Hyperthought = "HyperThought";
        #endregion

        #region Mathematics Fields
        /// <summary>
        /// Related to arithmetic (basic math) operations.
        /// </summary>
        public const string Arithmetic = "Arithmetic";
        /// <summary>
        /// Related to statistical operations.
        /// </summary>
        public const string Statistics = "Statistics";
        /// <summary>
        /// Related to number theoretical operations.
        /// </summary>
        public const string NumberTheory = "NumberTheory";
        #endregion
    }
}