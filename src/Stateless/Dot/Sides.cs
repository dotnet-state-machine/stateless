namespace Stateless.Core.Dot
{
    class Sides : Format
    {
        public Sides(int numberOfSides)
        {
            FormatName = "sides";
            FormatValue = numberOfSides.ToString();
        }
    }
}