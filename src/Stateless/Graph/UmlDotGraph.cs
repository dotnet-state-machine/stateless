using System;

using Stateless.Reflection;

namespace Stateless.Graph
{
    /// <summary>
    /// Class to generate a DOT grah in UML format
    /// </summary>
    public class UmlDotGraph
    {
        /// <summary>
        /// Generate a UML DOT graph from the state machine info
        /// </summary>
        /// <param name="machineInfo"></param>
        /// <returns></returns>
        public static string Format(StateMachineInfo machineInfo)
        {
            Graph.StateGraph graph = new Graph.StateGraph(machineInfo);

            return graph.ToGraph(new UmlDotGraphStyle());
        }

    }
}
