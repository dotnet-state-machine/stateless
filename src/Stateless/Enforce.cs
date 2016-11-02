using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stateless
{
    static class Enforce
    {
        public static T ArgumentNotNull<T>(T argument, string description)
            where T : class
        {
            if (argument == null)
                throw new ArgumentNullException(description);

            return argument;
        }
    }
}
