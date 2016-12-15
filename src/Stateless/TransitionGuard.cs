using System;
using System.Collections.Generic;
using System.Linq;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class TransitionGuard
        {
            internal IList<GuardCondition> Conditions { get; private set; }
            internal TransitionGuard(Tuple<Func<bool>, string>[] guards)
            {
                Conditions = guards
                    .Select(g => new GuardCondition(g.Item1, g.Item2))
                    .ToList();
            }
            internal TransitionGuard(Func<bool> guard = null, string guardDescription = null)
            {
                Conditions = new List<GuardCondition> { new GuardCondition(guard, guardDescription) };
            }
        }
    }
}