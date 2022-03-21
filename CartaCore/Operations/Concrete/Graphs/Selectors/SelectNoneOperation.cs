using System;
using System.Threading.Tasks;
using CartaCore.Graphs;
using CartaCore.Operations.Attributes;

namespace CartaCore.Operations.Graphs
{
    /// <summary>
    /// The parameters for the selector functionality of the <see cref="SelectNoneOperation"/> operation.
    /// </summary>
    public struct SelectNoneParameters { }
    /// <summary>
    /// The input for the <see cref="SelectNoneOperation"/> operation.
    /// </summary>
    public struct SelectNoneOperationIn
    {
        /// <summary>
        /// The graph to filter vertices on.
        /// </summary>
        [FieldName("Graph")]
        public Graph Graph { get; set; }
    }
    /// <summary>
    /// The output of the <see cref="SelectNoneOperation"/> operation.
    /// </summary>
    public struct SelectNoneOperationOut
    {
        /// <summary>
        /// The filtered graph.
        /// </summary>
        [FieldName("Graph")]
        public Graph Graph { get; set; }
    }
    
    /// <summary>
    /// Filters the vertices of a graph by selecting none of them.
    /// </summary>
    [OperationName(Display = "Select None", Type = "selectNone")]
    [OperationTag(OperationTags.Graph)]
    [OperationSelector("none")]
    public class SelectNoneOperation : TypedOperation
    <
        SelectNoneOperationIn,
        SelectNoneOperationOut
    >,
    ISelector<Graph, Graph>
    {
        /// <inheritdoc />
        public Type ParameterType => typeof(SelectNoneParameters);

        /// <inheritdoc />
        public override Task<SelectNoneOperationOut> Perform(SelectNoneOperationIn input)
        {
            return Task.FromResult(new SelectNoneOperationOut()
            {
                Graph = new MemoryGraph(input.Graph.Id, input.Graph.Properties)
            }); 
        }

        /// <inheritdoc />
        public async Task<Graph> Select(Graph source, object parameters)
        {
            if (parameters is SelectNoneParameters)
            {
                return (await Perform(new SelectNoneOperationIn()
                {
                    Graph = source
                })).Graph;
            }
            else throw new InvalidCastException();
        }
    }
}