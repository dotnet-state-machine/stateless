namespace Stateless.DotGraph
{
    /// <summary>
    /// Style of an item
    /// </summary>
    internal class Style : Format
    {
        internal Style(ShapeStyle value)
        {
            FormatName = "style";
            FormatValue = value.ToString();
        }
    }
}