using System;
using System.Collections.Generic;

using CartaCore.Data;

namespace CartaCore.Workflow.Action
{
    public class ActionReverseEdgesGraph : ActionGraph
    {
        protected override bool ShouldProvide(Type type)
        {
            if (type.IsAssignableTo(typeof(IRootedGraph))) return false;
            return base.ShouldProvide(type);
        }

        public override Edge TransformEdge(Edge edge)
        {
            return new Edge(edge.Target, edge.Source, edge.Properties);
        }
    }
}