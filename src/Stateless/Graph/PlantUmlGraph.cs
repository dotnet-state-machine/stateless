using Stateless.Reflection;

namespace Stateless.Graph
{
    /// <summary>
    /// Class to generate a graph in PlantUML format
    /// </summary>
    public static class PlantUmlGraph
    {
        /// <summary>
        /// Generate a PlantUML state diagram from the state machine info
        /// </summary>
        /// <param name="machineInfo">An object which exposes the states, transitions, and actions of the machine.</param>
        /// <param name="direction">
        /// When set, includes a direction setting in the output indicating the direction of flow.
        /// </param>
        /// <returns>The state machine as a PlantUML state diagram, enclosed in <c>@startuml</c> and <c>@enduml</c>.</returns>
        public static string Format(StateMachineInfo machineInfo, PlantUmlGraphDirection? direction = null)
        {
            var graph = new StateGraph(machineInfo);

            return graph.ToGraph(new PlantUmlGraphStyle(graph, direction));
        }
    }
}
