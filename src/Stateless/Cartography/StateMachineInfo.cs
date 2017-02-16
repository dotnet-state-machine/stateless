using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stateless.Cartography
{
    /// <summary>
    /// An info object which exposes the states, transitions, and actions of this machine.
    /// </summary>
    /// <typeparam name="TState">Type of the state in the machine to be mapped.</typeparam>
    /// <typeparam name="TTrigger">Type of the trigger in the machine to be mapped.</typeparam>
    public class StateMachineInfo<TState, TTrigger>
    {
        /// <summary>
        /// Construct an empty StateMachineInfo.
        /// </summary>
        public StateMachineInfo() { }

        internal StateMachineInfo(IEnumerable<StateResource<TState, TTrigger>> resources)
        {
            StateResources = resources;
        }

        /// <summary>
        /// Exposes the states, transitions, and actions of this machine.
        /// </summary>
        public IEnumerable<StateResource<TState, TTrigger>> StateResources { get; set; }
        
    }
}
