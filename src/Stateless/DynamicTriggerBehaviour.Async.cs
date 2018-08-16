#if TASKS

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal partial class DynamicTriggerBehaviour : TriggerBehaviour
        {
            readonly Func<object[], CancellationToken, Task<TState>> _destinationAsync;

            public DynamicTriggerBehaviour(TTrigger trigger, Func<object[], CancellationToken, Task<TState>> destination, 
                TransitionGuard transitionGuard, Reflection.DynamicTransitionInfo info)
                : base(trigger, transitionGuard)
            {
                _destinationAsync = destination ?? throw new ArgumentNullException(nameof(destination));
                TransitionInfo = info ?? throw new ArgumentNullException(nameof(info));
            }

            public override async Task<Tuple<bool, TState>> ResultsInTransitionFromAsync(TState source, object[] args, CancellationToken ct)
            {
                if (_destination != null)
                {
                    // Prefer synchronous state selector if available.
                    var destination = _destination(args);
                    return Tuple.Create(true, destination);
                }
                
                return Tuple.Create(true, await _destinationAsync(args, ct));
            }
        }
    }
}

#endif
