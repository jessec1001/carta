using System;
using System.Threading.Tasks;
using CartaCore.Graphs;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations.Graphs
{
    /// <summary>
    /// The parameters for the selector functionality of the <see cref="SelectAllOperation"/> operation.
    /// </summary>
    public struct SelectAllParameters { }
    /// <summary>
    /// The input for the <see cref="SelectAllOperation"/> operation.
    /// </summary>
    public struct SelectAllOperationIn
    {
        /// <summary>
        /// The graph to filter vertices on.
        /// </summary>
        [FieldName("Graph")]
        public Graph Graph { get; set; }
    }
    /// <summary>
    /// The output of the <see cref="SelectAllOperation"/> operation.
    /// </summary>
    public struct SelectAllOperationOut
    {
        /// <summary>
        /// The filtered graph.
        /// </summary>
        [FieldName("Graph")]
        public Graph Graph { get; set; }
    }
    
    /// <summary>
    /// Filters the vertices of a graph by selecting all of them.
    /// </summary>
    [OperationName(Display = "Select All", Type = "selectAll")]
    [OperationTag(OperationTags.Graph)]
    [OperationSelector("all")]
    public class SelectAllOperation : TypedOperation
    <
        SelectAllOperationIn,
        SelectAllOperationOut
    >,
    ISelector<Graph, Graph>
    {
        /// <inheritdoc />
        public Type ParameterType => typeof(SelectAllParameters);

        /// <inheritdoc />
        public override Task<SelectAllOperationOut> Perform(SelectAllOperationIn input)
        {
            return Task.FromResult(new SelectAllOperationOut() { Graph = input.Graph }); 
        }

        /// <inheritdoc />
        public async Task<Graph> Select(Graph source, object parameters)
        {
            if (parameters is SelectAllParameters)
            {
                return (await Perform(new SelectAllOperationIn()
                {
                    Graph = source
                })).Graph;
            }
            else throw new InvalidCastException();
        }
    }
}