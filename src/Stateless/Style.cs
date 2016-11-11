namespace Stateless.Dot
{
    class Style : Format
    {
        public Style(ShapeStyle value)
        {
            FormatName = "style";
            FormatValue = value.ToString();
        }
    }
}