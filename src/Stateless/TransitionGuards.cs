using System;
using System.Collections.Generic;
using System.Linq;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class GuardCondition
        {
            Func<bool> _guard;
            string _description;
            internal GuardCondition(Func<bool> guard, string description)
            {
                _guard = guard;
                _description = description;
            }
            internal Func<bool> Guard { get { return _guard; } }
            internal string Description { get { return _description ?? _guard.TryGetMethodName(); } }
        }

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