namespace Stateless.Core.Dot
{
    class Color : Format
    {
        public Color(HtmlColor htmlColor)
        {
            FormatName = "color";
            FormatValue = htmlColor.ToString();
        }
    }
}