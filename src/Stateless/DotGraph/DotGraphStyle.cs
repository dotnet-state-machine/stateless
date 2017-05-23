using System;
using System.Collections.Generic;
using System.Linq;

namespace Stateless.DotGraph
{
    /// <summary>
    /// Style definition for DotGraphFormatter
    /// </summary>
    public abstract class IDotGraphStyle
    {
        /// <summary>
        /// Style to be used for edges
        /// </summary>
        public ShapeStyle EdgeStyle { get; set; } = ShapeStyle.solid;

        /// <summary>
        /// Get the text that starts a new graph
        /// </summary>
        /// <returns></returns>
        abstract internal string GetPrefix();

        /// <summary>
        /// Generate the text for a single state
        /// </summary>
        /// <param name="sourceName"></param>
        /// <param name="entries"></param>
        /// <param name="exits"></param>
        /// <returns></returns>
        abstract internal string FormatOneState(string sourceName, List<String> entries, List<String> exits);

        /// <summary>
        /// Generate the text for a single decision node
        /// </summary>
        /// <param name="nodeName">Name of the node</param>
        /// <param name="label">Label for the node</param>
        /// <returns></returns>
        virtual internal string FormatOneDecisionNode(string nodeName, string label)
        {
            return nodeName + " [shape = \"diamond\", label = \"" + label + "\"]\n";
        }

        virtual internal string FormatOneLine(string fromNodeName, string toNodeName, string label)
        {
            FormatList edgeShape = new FormatList()
                .Add(new Style(EdgeStyle));

            if (label != null)
                edgeShape.Add(new Label(label));

            return fromNodeName + " -> " + toNodeName + " " + edgeShape;
        }
    }

    /// <summary>
    /// Generate DOT graphs in sle118 style
    /// </summary>
    public class SleGraphStyle : IDotGraphStyle
    {
        /// <summary>Get the text that starts a new graph</summary>
        /// <returns></returns>
        override internal string GetPrefix()
        { return $"digraph {{\ncompound=true;\nrankdir=\"LR\"\n"; }

        /// <summary>
        /// Generate the text for a single state
        /// </summary>
        /// <param name="sourceName"></param>
        /// <param name="entries"></param>
        /// <param name="exits"></param>
        /// <returns></returns>
        override internal string FormatOneState(string sourceName, List<String> entries, List<String> exits)
        {
            string label = $"\n\t<TABLE BORDER=\"1\" CELLBORDER=\"1\" CELLSPACING=\"0\" >";

            label += $"\n\t<tr><td>";

            if (entries.Count > 0)
            {
                label += $"\n\t\t<TABLE BORDER=\"0\" CELLBORDER=\"1\" CELLSPACING=\"0\" BGCOLOR=\"yellow\">";
                label += string.Join("", entries.Select(l => $"\n\t\t<TR><TD><sup>{l}</sup></TD></TR>"));
                label += $"\n\t\t</TABLE>\n";
            }

            label += $"\n\t</td></tr>";
            label += $"\n\t<TR><TD PORT=\"{sourceName}\">{sourceName}</TD></TR>";
            label += "<tr><td>";

            if (exits.Count > 0)
            {
                label += $"\n\t\t<TABLE BORDER=\"0\" CELLBORDER=\"1\" CELLSPACING=\"0\" BGCOLOR=\"yellow\">";
                label += string.Join("", exits.Select(l => $"\n\t\t<TR><TD><sup>{l}</sup></TD></TR>"));
                label += $"\n\t\t</TABLE>\n";
            }
            label += $"\n\t</td></tr>\n\t</TABLE>";

            FormatList _nodeShapeState = new FormatList()
                .Add(new Label(label).IsHTML())
                .Add(new Shape(ShapeType.plaintext))
                .Add(new Color(HtmlColor.blue))
                ;
            return $"\t{sourceName} {_nodeShapeState}\n";
        }
    }

    /// <summary>
    /// Generate DOT graphs in basic UML style
    /// </summary>
    public class UmlGraphStyle : IDotGraphStyle
    {
        /// <summary>Get the text that starts a new graph</summary>
        /// <returns></returns>
        override internal string GetPrefix()
        { return "digraph {\n"
                    + "compound=true;\n"
                    + "node [shape=Mrecord]\n"
                    + "rankdir=\"LR\"\n"; }

        /// <summary>
        /// Generate the text for a single state
        /// </summary>
        /// <param name="sourceName"></param>
        /// <param name="entries"></param>
        /// <param name="exits"></param>
        /// <returns></returns>
        override internal string FormatOneState(string sourceName, List<String> entries, List<String> exits)
        {
            if ((entries.Count == 0)&&(exits.Count == 0))
                return sourceName + " [label = \"" + sourceName + "\"]\n";

            string f = sourceName + " [label = \"" + sourceName + "|";

            List<string> es = new List<string>();
            foreach (string entry in entries)
                es.Add("entry / " + entry);
            foreach (string exit in exits)
                es.Add("exit / " + exit);

            f += String.Join("\\n", es);

            f += "\"]\n";

            return f;
        }
    }
}
