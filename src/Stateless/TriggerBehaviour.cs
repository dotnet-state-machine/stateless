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
            readonly TTrigger _trigger;
            readonly TransitionGuard _guard;

            protected TriggerBehaviour(TTrigger trigger, TransitionGuard guard)
            {
                _trigger = trigger;
                _guard = guard;

            }

            public TTrigger Trigger { get { return _trigger; } }
            internal ICollection<Func<bool>> Guards { get { return _guard.Conditions.Select(g => g.Guard).ToList(); } }
            internal string GuardsDescriptions
            {
                get
                {
                    var guardsDescriptions = _guard.Conditions
                        .Select(c => c.GuardDescription);

                    return string
                        .Join(",", guardsDescriptions);
                }
            }
            public bool IsGuardConditionMet
            {
                get {
                    return _guard.Conditions.All(c => c.Guard());
                }
            }
            public ICollection<string> UnmetGuardConditions
            {
                get
                {
                    return _guard.Conditions
                        .Where(c => !c.Guard())
                        .Select(c => c.GuardDescription)
                        .ToList();
                }
            }
            public abstract bool ResultsInTransitionFrom(TState source, object[] args, out TState destination);
        }
    }
}
