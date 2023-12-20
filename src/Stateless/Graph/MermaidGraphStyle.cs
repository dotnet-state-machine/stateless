using Stateless.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace Stateless.Graph
{
    /// <summary>
    /// Class to generate a graph in mermaid format
    /// </summary>
    public class MermaidGraphStyle : GraphStyleBase
    {
        /// <summary>
        /// Returns the formatted text for a single superstate and its substates.
        /// For example, for DOT files this would be a subgraph containing nodes for all the substates.
        /// </summary>
        /// <param name="stateInfo">The superstate to generate text for</param>
        /// <returns>Description of the superstate, and all its substates, in the desired format</returns>
        public override string FormatOneCluster(SuperState stateInfo)
        {
            string stateRepresentationString = "";
            return stateRepresentationString;
        }

        /// <summary>
        /// Generate the text for a single decision node
        /// </summary>
        /// <param name="nodeName">Name of the node</param>
        /// <param name="label">Label for the node</param>
        /// <returns></returns>
        public override string FormatOneDecisionNode(string nodeName, string label)
        {
            return String.Empty;
        }

        /// <summary>
        /// Generate the text for a single state
        /// </summary>
        /// <param name="state">The state to generate text for</param>
        /// <returns></returns>
        public override string FormatOneState(State state)
        {
            return String.Empty;
        }

        /// <summary>Get the text that starts a new graph</summary>
        /// <returns></returns>
        public override string GetPrefix()
        {
            return "stateDiagram-v2";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialState"></param>
        /// <returns></returns>
        public override string GetInitialTransition(StateInfo initialState)
        {
            return $"\r\n[*] --> {initialState}";
        }

        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceNodeName"></param>
        /// <param name="trigger"></param>
        /// <param name="actions"></param>
        /// <param name="destinationNodeName"></param>
        /// <param name="guards"></param>
        /// <returns></returns>
        public override string FormatOneTransition(string sourceNodeName, string trigger, IEnumerable<string> actions, string destinationNodeName, IEnumerable<string> guards)
        {
            string label = trigger ?? "";

            return FormatOneLine(sourceNodeName, destinationNodeName, label);
        }

        internal string FormatOneLine(string fromNodeName, string toNodeName, string label)
        {
            return $"\t{fromNodeName} --> {toNodeName} : {label}";
        }
    }
}
