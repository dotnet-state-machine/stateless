using System;
using System.Collections.Generic;

using Stateless.Reflection;

namespace Stateless.Graph
{
    /// <summary>
    /// Used to keep track of the decision point of a dynamic transition
    /// </summary>
    class Decision : State
    {
        public InvocationInfo Method { get; private set; }

        internal Decision(InvocationInfo method, int num)
            : base("Decision" + num.ToString())
        {
            Method = method;
        }
    }
}
