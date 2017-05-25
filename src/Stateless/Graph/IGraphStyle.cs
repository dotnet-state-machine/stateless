using System;
using System.Collections.Generic;

namespace Stateless.Graph
{
    /// <summary>
    /// Style definition for DotGraphFormatter
    /// </summary>
    public abstract class IGraphStyle
    {
        /// <summary>
        /// Get the text that starts a new graph
        /// </summary>
        /// <returns></returns>
        abstract internal string GetPrefix();

        /// <summary>
        /// Generate the text for a single state
        /// </summary>
        /// <param name="state">The state to generate text for</param>
        /// <returns></returns>
        abstract internal string FormatOneState(Stateless.Graph.State state);

        abstract internal string FormatOneCluster(Stateless.Graph.SuperState stateInfo);

        abstract internal List<string> FormatAllTransitions(List<Stateless.Graph.Transition> transitions);

        abstract internal string FormatOneTransition(string source, string trigger, IEnumerable<string> actions, string destinationString, IEnumerable<string> guards);

        /// <summary>
        /// Generate the text for a single decision node
        /// </summary>
        /// <param name="nodeName">Name of the node</param>
        /// <param name="label">Label for the node</param>
        /// <returns></returns>
        abstract internal string FormatOneDecisionNode(string nodeName, string label);

        abstract internal string FormatOneLine(string fromNodeName, string toNodeName, string label);
    }
}
