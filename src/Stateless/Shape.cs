namespace Stateless.Dot
{
    class Shape : Format
    {
        public Shape(ShapeType shapeType)
        {
            FormatName = "shape";
            FormatValue = shapeType.ToString();
        }
    }
}