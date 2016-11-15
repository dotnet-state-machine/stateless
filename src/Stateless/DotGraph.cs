using Stateless.Core.Cartography;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        /// <summary>
        /// A string representation of the state machine in the DOT graph language.
        /// </summary>
        /// <returns>A description of all simple source states, triggers and destination states.</returns>
        public string ToDotGraph()
        {
            return WriteMap(new DotGraphCartographer<TState, TTrigger>());
        }

    }
}
