using System;
using System.Collections.Generic;
using System.Linq;

namespace Stateless.Reflection
{
    /// <summary>
    /// An info object which exposes the states, transitions, and actions of this machine.
    /// </summary>
    public class StateMachineInfo
    {
        internal StateMachineInfo(IEnumerable<StateInfo> states)
        {
            States = states?.ToList() ?? throw new ArgumentNullException(nameof(states));            
        }

        /// <summary>
        /// Exposes the states, transitions, and actions of this machine.
        /// </summary>
        public IEnumerable<StateInfo> States { get; }
    }
}
