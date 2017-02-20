using System;

namespace Stateless.Reflection
{
    /// <summary>
    /// Represents a component that can parse StateMachineInfo.
    /// </summary>
    public interface IStateMachineInfoFormatter
    {
        /// <summary>
        /// Discover and produce a textual representation of a StateMachineInfo.
        /// </summary>
        /// <param name="stateMachineInfo">StateMachine instance to be discovered.</param>
        /// <returns>Textual representation of the StateMachine.</returns>
        string Format(StateMachineInfo stateMachineInfo);
    }
}
