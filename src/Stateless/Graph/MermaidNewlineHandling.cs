namespace Stateless.Graph
{
    /// <summary>
    /// How line breaks in state names, triggers and guard descriptions are written to a Mermaid graph.
    /// </summary>
    public enum MermaidNewlineHandling
    {
        /// <summary>
        /// Replace each line break with an HTML <c>&lt;br/&gt;</c> tag, so that it is shown as a line break
        /// in the rendered diagram. This is the default.
        /// </summary>
        Html,
        /// <summary>Replace each line break with a single space.</summary>
        Space
    }
}
