namespace Stateless.DotGraph
{
    class FixedSize : Format
    {
        public FixedSize(bool fixedSize)
        {
            FormatName = "fixedsize";
            FormatValue = fixedSize.ToString();
        }
    }
}