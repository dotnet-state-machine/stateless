using Stateless.Reflection;

namespace Stateless.Graph
{
    /// <summary>
    /// Class to generate a DOT grah in UML format
    /// </summary>
    public static class UmlDotGraph
    {
        /// <summary>
        /// Generate a UML DOT graph from the state machine info
        /// </summary>
        /// <param name="machineInfo"></param>
        /// <returns></returns>
        public static string Format(StateMachineInfo machineInfo)
        {
            var graph = new StateGraph(machineInfo);

            return graph.ToGraph(new UmlDotGraphStyle());
        }

    }
}
