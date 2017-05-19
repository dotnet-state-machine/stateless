using System;
using System.Collections.Generic;
using System.Linq;

namespace Stateless.DotGraph
{
    /// <summary>
    /// Style definition for DotGraphFormatter
    /// </summary>
    public interface IDotGraphStyle
    {
        /// <summary>
        /// Get the text that starts a new graph
        /// </summary>
        /// <returns></returns>
        string GetPrefix();

        /// <summary>
        /// Generate the text for a single state
        /// </summary>
        /// <param name="sourceName"></param>
        /// <param name="entries"></param>
        /// <param name="exits"></param>
        /// <returns></returns>
        string FormatOneState(string sourceName, List<String> entries, List<String> exits);
    }

    /// <summary>
    /// Generate DOT graphs in sle118 style
    /// </summary>
    public class SleGraphStyle : IDotGraphStyle
    {
        /// <summary>Get the text that starts a new graph</summary>
        /// <returns></returns>
        string IDotGraphStyle.GetPrefix()
        { return $"digraph {{\ncompound=true;\nrankdir=\"LR\"\n"; }

        /// <summary>
        /// Generate the text for a single state
        /// </summary>
        /// <param name="sourceName"></param>
        /// <param name="entries"></param>
        /// <param name="exits"></param>
        /// <returns></returns>
        string IDotGraphStyle.FormatOneState(string sourceName, List<String> entries, List<String> exits)
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
        /*
	A [   label=<
	<TABLE BORDER="1" CELLBORDER="1" CELLSPACING="0" >
	<tr><td>
	</td></tr>
	<TR><TD PORT="A">A</TD></TR><tr><td>
	</td></tr>
	</TABLE>>,shape="plaintext",color="blue" ];
         */
        /*
            string label = $"\n\t<TABLE BORDER=\"1\" CELLBORDER=\"1\" CELLSPACING=\"0\" >";

            label += $"\n\t<tr><td>";
            if (stateInfo != null && ProcessEntries(stateInfo).Any())
            {
                label += $"\n\t\t<TABLE BORDER=\"0\" CELLBORDER=\"1\" CELLSPACING=\"0\" BGCOLOR=\"yellow\">";
                label += string.Join("", ProcessEntries(stateInfo).Select(l => $"\n\t\t<TR><TD><sup>{l}</sup></TD></TR>"));
                label += $"\n\t\t</TABLE>\n";
            }

            label += $"\n\t</td></tr>";
            label += $"\n\t<TR><TD PORT=\"{sourceName}\">{sourceName}</TD></TR>";
            label += "<tr><td>";
            if (stateInfo != null && ProcessExits(stateInfo).Any())
            {
                label += $"\n\t\t<TABLE BORDER=\"0\" CELLBORDER=\"1\" CELLSPACING=\"0\" BGCOLOR=\"yellow\">";
                label += string.Join("", ProcessExits(stateInfo).Select(l => $"\n\t\t<TR><TD><sup>{l}</sup></TD></TR>"));
                label += $"\n\t\t</TABLE>\n";
            }
            label += $"\n\t</td></tr>\n\t</TABLE>";

            _nodeShapeState = new FormatList()
                .Add(new Label(label).IsHTML())
                .Add(new Shape(ShapeType.plaintext))
                .Add(new Color(HtmlColor.blue))
                ;
            return $"\t{sourceName} {_nodeShapeState}\n";
         */
    }

    /// <summary>
    /// Generate DOT graphs in basic UML style
    /// </summary>
    public class UmlGraphStyle : IDotGraphStyle
    {
        /// <summary>Get the text that starts a new graph</summary>
        /// <returns></returns>
        string IDotGraphStyle.GetPrefix()
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
        public string FormatOneState(string sourceName, List<String> entries, List<String> exits)
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
