namespace Stateless.Dot
{
    class Color : Format
    {
        public Color(ShapeColor shapeColor)
        {
            FormatName = "color";
            FormatValue = shapeColor.ToString();
        }
    }
}