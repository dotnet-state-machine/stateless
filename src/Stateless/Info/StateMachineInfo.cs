using System;
using System.Collections.Generic;
using System.Reflection;

namespace Stateless.Reflection
{
    /// <summary>
    /// An info object which exposes the states, transitions, and actions of this machine.
    /// </summary>
    public class StateMachineInfo
    {
        /// <summary>
        /// Construct an empty StateMachineInfo.
        /// </summary>
        public StateMachineInfo() { }

        internal StateMachineInfo(ICollection<StateBindingInfo> states)
        {
            StateBindings = states;            
        }

        /// <summary>
        /// Exposes the states, transitions, and actions of this machine.
        /// </summary>
        public ICollection<StateBindingInfo> StateBindings { get; set; } = new List<StateBindingInfo>();
        
    }
}
