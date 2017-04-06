using System;
using System.Collections.Generic;
using System.Linq;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class TransitionGuard
        {
            internal IList<GuardCondition> Conditions { get; }
            internal TransitionGuard(Tuple<Func<bool>, string>[] guards)
            {
                Enforce.ArgumentNotNull(guards, nameof(guards));

                Conditions = guards
                    .Select(g => new GuardCondition(g.Item1, g.Item2))
                    .ToList();
            }

            internal TransitionGuard(Func<bool> guard, string description = null)
            {
                Enforce.ArgumentNotNull(guard, nameof(guard));

                Conditions = new List<GuardCondition> { new GuardCondition(guard, description) };
            }
        }
    }
}