namespace Stateless.Core.Dot
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