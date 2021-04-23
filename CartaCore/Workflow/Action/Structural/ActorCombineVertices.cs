using System;
using System.Collections.Generic;
using System.Linq;

using MorseCode.ITask;

using CartaCore.Data;
using CartaCore.Serialization;

namespace CartaCore.Workflow.Action
{
    [DiscriminantDerived("combineVertices")]
    public class ActorCombineVertices : Actor,
        IDynamicGraph<IVertex>,
        IEntireGraph
    {
        public string Name { get; set; }

        protected override bool ShouldProvide(Type type)
        {
            if (type.IsAssignableTo(typeof(IRootedGraph))) return false;
            if (type.IsAssignableTo(typeof(IDynamicGraph<IVertex>))) return Graph.CanProvide<IDynamicGraph<IVertex>>();
            if (type.IsAssignableTo(typeof(IEntireGraph))) return Graph.CanProvide<IEntireGraph>();
            return base.ShouldProvide(type);
        }

        private async ITask<IVertex> GetCombinedVertex(Identity id)
        {
            IDynamicGraph<IVertex> dynamic = Graph.Provide<IDynamicGraph<IVertex>>();

            List<Property> properties = new List<Property>();
            List<Edge> inEdges = new List<Edge>();
            List<Edge> outEdges = new List<Edge>();
            await foreach (IVertex subvertex in Selector.GetVertices())
            {
                if (id is null) id = subvertex.Identifier;
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
                            inEdges.Add(inEdge);
                    }
                }
                if (subvertex is IOutVertex outVertex)
                {
                    foreach (Edge outEdge in outVertex.OutEdges)
                    {
                        IVertex targetVertex = await dynamic.GetVertex(outEdge.Target);
                        if (!await Selector.ContainsVertex(targetVertex))
                            outEdges.Add(outEdge);
                    }
                }
            }
            IVertex vertex = new InOutVertex(id, properties, inEdges, outEdges);
            return vertex;
        }

        public async ITask<IVertex> GetVertex(Identity id)
        {
            if (Graph.TryProvide(out IDynamicGraph<IVertex> dynamic))
            {
                IVertex vertex = await dynamic.GetVertex(id);
                if (await Selector.ContainsVertex(vertex))
                    return await GetCombinedVertex(null);
                else return vertex;
            }
            else throw new NotSupportedException();
        }
        public async IAsyncEnumerable<IVertex> GetVertices()
        {
            if (Graph.TryProvide(out IEntireGraph entire))
            {
                await foreach (IVertex vertex in entire.GetVertices())
                {
                    if (await Selector.ContainsVertex(vertex))
                        yield return await GetCombinedVertex(null);
                    else yield return vertex;
                }
            }
        }
    }
}