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
            readonly TransitionGuard _guard;

            protected TriggerBehaviour(TTrigger trigger, TransitionGuard guard)
            {
                _guard = Enforce.ArgumentNotNull(guard, nameof(guard));
                Trigger = trigger;
            }

            public TTrigger Trigger { get; }
            internal ICollection<Func<bool>> Guards { get { return _guard.Conditions.Select(g => g.Guard).ToList(); } }
            internal string GuardsDescriptions
            {
                get
                {
                    var guardsDescriptions = _guard.Conditions
                        .Select(c => c.Description);

                    return string
                        .Join(",", guardsDescriptions);
                }
            }
            public bool GuardConditionsMet
            {
                get
                {
                    return _guard.Conditions.All(c => c.Guard());
                }
            }
            public ICollection<string> UnmetGuardConditions
            {
                get
                {
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
