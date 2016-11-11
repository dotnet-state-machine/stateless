namespace Stateless.Dot
{
    class Format
    {
        protected string FormatName;
        protected string FormatValue;
        private bool _isHtml;

        public override string ToString()
        {
            if (_isHtml)
                return $"{FormatName}=<{FormatValue}>";
            else
                return $"{FormatName}=\"{FormatValue}\"";
        }

        public Format IsHTML(bool isHtml=true)
        {
            _isHtml = isHtml;
            return this;
        }

        protected bool Equals(Format other)
        {
            return string.Equals(FormatName, other.FormatName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Format) obj);
        }

        public override int GetHashCode()
        {
            return (FormatName != null ? FormatName.GetHashCode() : 0);
        }

        public static bool operator ==(Format left, Format right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Format left, Format right)
        {
            return !Equals(left, right);
        }
    }
}