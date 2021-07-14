using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using MorseCode.ITask;
using NJsonSchema.Annotations;

using CartaCore.Data;
using CartaCore.Serialization;

namespace CartaCore.Workflow.Action
{
    /// <summary>
    /// Combines the currently selected vertices into a single combined vertex.
    /// </summary>
    [JsonSchemaFlatten]
    [DataContract]
    [DiscriminantDerived("combineVertices")]
    [DiscriminantSemantics(Name = "Combine Vertices", Group = "Structural", Hidden = true)]
    public class ActorCombineVertices : Actor,
        IDynamicGraph<IVertex>,
        IEntireGraph
    {
        /// <summary>
        /// The unique identifier to assign to the newly combined vertex. 
        /// </summary>
        [DataMember(Name = "id")]
        [Display(Name = "Id")]
        [Required]
        public string Id { get; set; }
        /// <summary>
        /// The label to assign to the newly combined vertex.
        /// </summary>
        [DataMember(Name = "name")]
        [Display(Name = "Name")]
        [Required]
        public string Name { get; set; }

        /// <inheritdoc />
        protected override bool ShouldProvide(Type type)
        {
            if (type.IsAssignableTo(typeof(IRootedGraph))) return false;
            if (type.IsAssignableTo(typeof(IDynamicGraph<IVertex>))) return Graph.CanProvide<IDynamicGraph<IVertex>>();
            if (type.IsAssignableTo(typeof(IEntireGraph))) return Graph.CanProvide<IEntireGraph>();
            return base.ShouldProvide(type);
        }

        private async ITask<IVertex> GetCombinedVertex()
        {
            IDynamicGraph<IVertex> dynamic = Graph.Provide<IDynamicGraph<IVertex>>();

            List<Property> properties = new List<Property>();
            List<Edge> inEdges = new List<Edge>();
            List<Edge> outEdges = new List<Edge>();
            await foreach (IVertex subvertex in Selector.GetVertices())
            {
                foreach (Property property in subvertex.Properties)
                {
                    Property combinedProperty = properties.FirstOrDefault(prop => prop.Identifier.Equals(property.Identifier));
                    if (combinedProperty is null)
                        combinedProperty = new Property(property.Identifier);

                    combinedProperty.Values = combinedProperty.Values.Append(property.Values);
                }
                if (subvertex is IInVertex inVertex)
                {
                    foreach (Edge inEdge in inVertex.InEdges)
                    {
                        IVertex sourceVertex = await dynamic.GetVertex(inEdge.Source);
                        if (!await Selector.ContainsVertex(sourceVertex))
                            inEdges.Add(new Edge(inEdge.Identifier, inEdge.Source, Identity.Create(Id), inEdge.Properties));
                    }
                }
                if (subvertex is IOutVertex outVertex)
                {
                    foreach (Edge outEdge in outVertex.OutEdges)
                    {
                        IVertex targetVertex = await dynamic.GetVertex(outEdge.Target);
                        if (!await Selector.ContainsVertex(targetVertex))
                            outEdges.Add(new Edge(outEdge.Identifier, Identity.Create(Id), outEdge.Target, outEdge.Properties));
                    }
                }
            }
            IVertex vertex = new InOutVertex(Identity.Create(Id), properties, inEdges, outEdges)
            {
                Label = Name
            };
            return vertex;
        }

        /// <inheritdoc />
        public override async Task<Edge> TransformEdge(Edge edge)
        {
            if (Graph.TryProvide(out IDynamicGraph<IVertex> dynamic))
            {
                IVertex source = await dynamic.GetVertex(edge.Source);
                IVertex target = await dynamic.GetVertex(edge.Target);

                Identity sourceId = edge.Source;
                Identity targetId = edge.Target;

                if (await Selector.ContainsVertex(source))
                    sourceId = Identity.Create(Id);
                if (await Selector.ContainsVertex(target))
                    targetId = Identity.Create(Id);
                return new Edge(edge.Identifier, sourceId, targetId, edge.Properties);
            }
            else throw new NotSupportedException();
        }

        /// <inheritdoc />
        public new async ITask<IVertex> GetVertex(Identity id)
        {
            if (Graph.TryProvide(out IDynamicGraph<IVertex> dynamic))
            {
                IVertex vertex = await dynamic.GetVertex(id);
                if (await Selector.ContainsVertex(vertex))
                    return await GetCombinedVertex();
                else return vertex;
            }
            else throw new NotSupportedException();
        }
        /// <inheritdoc />
        public new async IAsyncEnumerable<IVertex> GetVertices()
        {
            if (Graph.TryProvide(out IEntireGraph entire))
            {
                await foreach (IVertex vertex in entire.GetVertices())
                {
                    if (await Selector.ContainsVertex(vertex))
                        yield return await GetCombinedVertex();
                    else yield return vertex;
                }
            }
        }
    }
}