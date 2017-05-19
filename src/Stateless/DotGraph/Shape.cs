
namespace Stateless.DotGraph
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