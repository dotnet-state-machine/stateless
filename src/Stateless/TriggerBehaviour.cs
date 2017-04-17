using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal abstract class TriggerBehaviour
        {
            readonly TransitionGuard _guard;        // null if there is no guard function

            protected TriggerBehaviour(TTrigger trigger, TransitionGuard guard)
            {
                _guard = guard;
                Trigger = trigger;
            }

            public TTrigger Trigger { get; }

            internal ICollection<Func<bool>> Guards
            {
                get
                {
                    if (_guard == null)
                        return new List<Func<bool>>();
                    else
                        return _guard.Conditions.Select(g => g.Guard).ToList();
                }
            }

            internal TransitionGuard Guard => _guard;       // null if there is no guard function

            public bool GuardConditionsMet
            {
                get
                {
                    return (_guard == null) || _guard.Conditions.All(c => c.Guard());
                }
            }
            public ICollection<string> UnmetGuardConditions
            {
                get
                {
                    if (_guard == null)
                        return new List<string>();
                    return _guard.Conditions
                        .Where(c => !c.Guard())
                        .Select(c => c.Description)
                        .ToList();
                }
            }
            public abstract bool ResultsInTransitionFrom(TState source, object[] args, out TState destination);
        }
    }
}
