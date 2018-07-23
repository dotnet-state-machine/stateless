#if TASKS

using System;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal partial class DynamicTriggerBehaviour : TriggerBehaviour
        {
            readonly Func<object[], Task<TState>> _destinationAsync;

            public DynamicTriggerBehaviour(TTrigger trigger, Func<object[], Task<TState>> destination, 
                TransitionGuard transitionGuard, Reflection.DynamicTransitionInfo info)
                : base(trigger, transitionGuard)
            {
                _destinationAsync = destination ?? throw new ArgumentNullException(nameof(destination));
                TransitionInfo = info ?? throw new ArgumentNullException(nameof(info));
            }

            public override async Task<Tuple<bool, TState>> ResultsInTransitionFromAsync(TState source, object[] args)
            {
                return Tuple.Create(true, await _destinationAsync(args));
            }
        }
    }
}

#endif
