#if TASKS

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal abstract partial class TriggerBehaviour
        {
            /// <summary>
            /// UnmetGuardConditions is a list of the descriptions of all guard conditions
            /// whose guard function returns false
            /// </summary>
            public Task<ICollection<string>> UnmetGuardConditionsAsync(object[] args) => _guard.UnmetGuardConditionsAsync(args);

            public virtual Task<Tuple<bool, TState>> ResultsInTransitionFromAsync(TState source, object[] args, CancellationToken ct)
            {
                var result = ResultsInTransitionFrom(source, args, out var destinationState);
                return TaskResult.FromResult(Tuple.Create(result, destinationState));
            }
        }
    }
}

#endif
