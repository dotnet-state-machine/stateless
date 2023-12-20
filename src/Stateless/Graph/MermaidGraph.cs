using Stateless.Reflection;

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
        /// <returns></returns>
        public static string Format(StateMachineInfo machineInfo)
        {
            var graph = new StateGraph(machineInfo);

            return graph.ToGraph(new MermaidGraphStyle());
        }

    }
}
