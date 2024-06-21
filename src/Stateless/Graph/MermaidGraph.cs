using Stateless.Reflection;
using System.Collections;

namespace Stateless.Graph
{
    /// <summary>
    /// Class to generate a MermaidGraph
    /// </summary>
    public static class MermaidGraph
    {
        /// <summary>
        /// Generate a Mermaid graph from the state machine info
        /// </summary>
        /// <param name="machineInfo"></param>
        /// <param name="direction">
        /// When set, includes a <c>direction</c> setting in the output indicating the direction of flow.
        /// </param>
        /// <returns></returns>
        public static string Format(StateMachineInfo machineInfo, MermaidGraphDirection? direction = null)
        {
            var graph = new StateGraph(machineInfo);

            return graph.ToGraph(new MermaidGraphStyle(graph, direction));
        }
    }
}
