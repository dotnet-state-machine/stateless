using System;
using System.Collections.Generic;
using System.Linq;

namespace Stateless.Dot
{
    class FormatList
    {
        private readonly List<Format> _formats;
        private string _ltail;
        private string _lhead;

        public FormatList()
        {
            _formats = new List<Format>();
        }

        public FormatList Add(Format format)
        {
            if (_formats.Any(f => f == format))
                _formats.Remove(format);

            _formats.Add(format);
            return this;
        }

        public FormatList SetLTail(string ltail, string resolvedName)
        {
            if(!string.IsNullOrEmpty(ltail)
               && resolvedName != ltail)
                _ltail= $"ltail = {resolvedName},";
            else
            {
                _ltail = string.Empty;
            }
            return this;
        }

        /// <summary>
        /// sets the origin of the edge. Gets reset when "ToString" is called
        /// </summary>
        /// <param name="lhead"></param>
        /// <param name="resolvedName"></param>
        /// <returns></returns>
        public FormatList SetLHead(string lhead, string resolvedName)
        {
            if (!string.IsNullOrEmpty(lhead)
                && resolvedName != lhead)
                _lhead = $"lhead = {resolvedName},";
            else
            {
                _lhead = string.Empty;
            }
            return this;
        }
        public override string ToString()
        {
            if (_formats.Count>0)
            {
                var outString = $"[ {_ltail} {_lhead} {String.Join(",", _formats)} ];";
                _ltail = "";
                _lhead = "";
                return outString;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}