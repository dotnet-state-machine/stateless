using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stateless.Cartography
{
    /// <summary>
    /// An exlorer that uses a parellel tree-structure to represent internal states.
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TTrigger"></typeparam>
    class StateResourceExplorer<TState, TTrigger> : IExplorer<TState, TTrigger>
    {
        public ICollection<StateResource<TState, TTrigger>> Discover(IDictionary<TState, StateMachine<TState, TTrigger>.StateRepresentation> representations)
        {
            return representations.Select(kvp => new StateResource<TState, TTrigger>(kvp.Value)).ToList();
        }
    }
}
