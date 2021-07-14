using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using CartaCore.Data;
using CartaCore.Serialization;

using NJsonSchema.Annotations;

namespace CartaCore.Workflow.Selection
{
    /// <summary>
    /// Selects vertices with a specified indegree and outdegree which defines the number of edges that may be connected
    /// to the selected vertices.
    /// </summary>
    [JsonSchemaFlatten]
    [DataContract]
    [DiscriminantDerived("degree")]
    [DiscriminantSemantics(Name = "Select By Degree", Group = "Structural")]
    public class SelectorDegree : Selector
    {
        #region API Parameters
        /// <summary>
        /// The [indegree](https://en.wikipedia.org/wiki/Directed_graph#Indegree_and_outdegree) of vertices to select.
        /// - If set to zero, only the vertices with no directed in-edges will be selected.
        /// - If set to null, vertices with any number of in-edges will be selected.
        /// 
        /// Defaults to null. 
        /// </summary>
        [DataMember(Name = "inDegree")]
        [Display(Name = "Indegree")]
        [Range(0d, double.PositiveInfinity)]
        public int? InDegree { get; set; } = null;
        /// <summary>
        /// The [outdegree](https://en.wikipedia.org/wiki/Directed_graph#Indegree_and_outdegree) of vertices to select.
        /// - If set to zero, only the vertices with no directed out-edges will be selected.
        /// - If set to null, vertices with any number of in-edges will be selected.
        /// 
        /// Defaults to null. 
        /// </summary>
        [DataMember(Name = "outDegree")]
        [Display(Name = "Outdegree")]
        [Range(0d, double.PositiveInfinity)]
        public int? OutDegree { get; set; } = null;
        #endregion

        /// <inheritdoc />
        public override Task<bool> ContainsVertex(IVertex vertex)
        {
            if (InDegree.HasValue && (
                vertex is not IInVertex inVertex ||
                inVertex.InEdges.Count() != InDegree
            )) return Task.FromResult(false);
            if (OutDegree.HasValue && (
                vertex is not IOutVertex outVertex ||
                outVertex.OutEdges.Count() != OutDegree
            )) return Task.FromResult(false);
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public override IAsyncEnumerable<IVertex> GetVertices()
        {
            if (
                InDegree == 0 && OutDegree == null &&
                Graph.TryProvide(out IRootedGraph rooted) &&
                Graph.TryProvide(out IDynamicGraph<IVertex> dynamic)
            ) return dynamic.GetVertices(rooted.GetRoots());
            else return base.GetVertices();
        }

        /// <summary>
        /// A vertex degree selector that only selects root vertices which have no in-edges.
        /// </summary>
        [DiscriminantAlias("roots")]
        public static SelectorDegree CreateRootsSelector() => new SelectorDegree
        {
            InDegree = 0,
            OutDegree = null
        };
    }
}