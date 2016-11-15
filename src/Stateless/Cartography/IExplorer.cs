using System.Collections.Generic;

namespace Stateless.Cartography
{
    /// <summary>
    /// Represents a translation-layer to change internal representations into publicly available ones.
    /// </summary>
    /// <typeparam name="TState">Type of the state in the machine to be mapped.</typeparam>
    /// <typeparam name="TTrigger">Type of the trigger in the machine to be mapped.</typeparam>
    interface IExplorer<TState, TTrigger>
    {
        /// <summary>
        /// Translate internal representations into publicly available ones.
        /// </summary>
        /// <param name="representations">The state representations available.</param>
        /// <returns>All resources found.</returns>
        ICollection<StateResource<TState, TTrigger>> Discover(IDictionary<TState, StateMachine<TState, TTrigger>.StateRepresentation> representations);
    }
}
