using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stateless.Cartography
{
    /// <summary>
    /// Represents a component that can parse StateMachineInfo.
    /// </summary>
    /// <typeparam name="TState">Type of the state in the machine to be mapped.</typeparam>
    /// <typeparam name="TTrigger">Type of the trigger in the machine to be mapped.</typeparam>
    public interface ICartographer<TState, TTrigger>
    {
        /// <summary>
        /// Discover and produce a textual representation of a StateMachineInfo.
        /// </summary>
        /// <param name="stateMachineInfo">StateMachine instance to be discovered.</param>
        /// <returns>Textual representation of the StateMachine.</returns>
        string WriteMap(StateMachineInfo<TState, TTrigger> stateMachineInfo);
    }
}
